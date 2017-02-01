// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.LogFileManagerTests
{
    using System;
    using System.IO;
    using System.Linq;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;
    using Lifti.Tests.Persistence.DataFileManagerTests;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for reading the original data file extent from a <see cref="LogFileManager"/> instance.
    /// </summary>
    [TestClass]
    public class ReadingOriginalDataFileExtent : UnitTestBase
    {
        /// <summary>
        /// The path to the test file.
        /// </summary>
        private string path;

        /// <summary>
        /// Cleans up before and after each test.
        /// </summary>
        [TestCleanup]
        [TestInitialize]
        public void TestCleanup()
        {
            this.path = $"testindex{Guid.NewGuid()}.dat";

            if (File.Exists(this.path))
            {
                File.Delete(this.path);
            }

            if (File.Exists(this.path + ".txlog"))
            {
                File.Delete(this.path + ".txlog");
            }
        }

        /// <summary>
        /// An exception should be thrown if no transaction is logged and the original extent is read.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfNoTransactionLogged()
        {
            using (var logFileManager = new LogFileManager(this.path))
            {
                int originalExtent;
                AssertRaisesException<InvalidOperationException>(
                    () => originalExtent = logFileManager.OriginalDataFileExtent,
                    "Unable to read original data file extent from log - no extent data logged.");
            }
        }

        /// <summary>
        /// The correct extent value should be read from the correct location.
        /// </summary>
        [TestMethod]
        public void ShouldReturnCorrectExtentValue()
        {
            var headerBytes = Data.LogFileHeader(TransactionLogState.Incomplete, 233).ToArray();
            FileHelper.CreateFile(this.path, headerBytes);

            using (var logFileManager = new LogFileManager(this.path))
            {
                var originalExtent = logFileManager.OriginalDataFileExtent;
                Assert.AreEqual(233, originalExtent);
            }
        }

        /// <summary>
        /// When reading the original extent value, subsequent writes resume from where previous writes left off.
        /// </summary>
        [TestMethod]
        public void ShouldNotAffectLogLocation()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);
            dataFile.Setup(d => d.ReadRaw(10, It.Is<byte[]>(b => b.Length == 20))).Returns((int i, byte[] b) => b);
            dataFile.Setup(d => d.ReadRaw(30, It.Is<byte[]>(b => b.Length == 10))).Returns((int i, byte[] b) => b);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
                Assert.AreEqual(Data.LogHeaderSize, logFileManager.CurrentLength);

                logFileManager.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 20);
                Assert.AreEqual(Data.LogHeaderSize + 20 + Data.LogEntryHeaderSize, logFileManager.CurrentLength);

                var originalExtent = logFileManager.OriginalDataFileExtent;
                Assert.AreEqual(233, originalExtent);

                logFileManager.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 30, 10);

                Assert.AreEqual(Data.LogHeaderSize + 30 + (Data.LogEntryHeaderSize * 2), logFileManager.CurrentLength);
            }
        }
    }
}

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
    /// Tests the initialization of a new transaction on the <see cref="LogFileManager"/> class.
    /// </summary>
    [TestClass]
    public class InitializingNewTransaction : UnitTestBase
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
        /// If a null data file manager is provided, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfNullDataFileManagerProvided()
        {
            using (var logFileManager = new LogFileManager(this.path))
            {
                AssertRaisesArgumentNullException(() => logFileManager.InitializeNewLog(null), "dataFileManager");
            }
        }

        /// <summary>
        /// When initializing a new transaction, the expected data should be written out in the header.
        /// </summary>
        [TestMethod]
        public void ShouldWriteExpectedDataOutInHeader()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(299);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
            }

            Assert.IsTrue(Data.LogFileHeader(TransactionLogState.Incomplete, 299).SequenceEqual(FileHelper.GetFileBytes(this.path)));
        }

        /// <summary>
        /// The state of the log file should be marked as incomplete after initialization.
        /// </summary>
        [TestMethod]
        public void ShouldMarkTheStateAsIncomplete()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(299);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);

                Assert.AreEqual(TransactionLogState.Incomplete, logFileManager.LogState);
            }
        }
    }
}

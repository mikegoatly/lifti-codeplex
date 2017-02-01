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
    /// Tests for changing (and indirectly, reading) the state of the log file.
    /// </summary>
    [TestClass]
    public class ChangingLogState
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
        /// The state of the log file should be written to the log file.
        /// </summary>
        [TestMethod]
        public void ShouldWriteStateToLogFile()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
                logFileManager.LogState = TransactionLogState.None;
            }

            var expectedData = Data.LogFileHeader(TransactionLogState.None, 233);

            var actual = FileHelper.GetFileBytes(this.path);

            Assert.IsTrue(expectedData.SequenceEqual(actual));
        }

        /// <summary>
        /// When the state is changed, the new value should be returned by the property.
        /// </summary>
        [TestMethod]
        public void ChangedStatusShouldBeReturnedByTheProperty()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
                logFileManager.LogState = TransactionLogState.None;

                Assert.AreEqual(TransactionLogState.None, logFileManager.LogState);
            }
        }
    }
}

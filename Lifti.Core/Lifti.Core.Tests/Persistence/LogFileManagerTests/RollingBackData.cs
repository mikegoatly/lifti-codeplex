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
    /// Tests for rolling data back to the data store from a log file.
    /// </summary>
    [TestClass]
    public class RollingBackData : UnitTestBase
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
                AssertRaisesArgumentNullException(() => logFileManager.RollbackDataTo(null), "dataFileManager");
            }
        }

        /// <summary>
        /// The state of the log file should be written to the log file.
        /// </summary>
        [TestMethod]
        public void ShouldWriteStateToLogFile()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.Incomplete, 233)
                .Then(Data.LogEntryHeader(LogEntryDataType.PageManagerHeader, 0, 3))
                .Then(new byte[] { 1, 2, 3 })
                .Then(Data.LogEntryHeader(LogEntryDataType.PageHeader, 3, 2))
                .Then(new byte[] { 4, 5 })
                .Then(Data.LogEntryHeader(LogEntryDataType.FullPage, 5, 4))
                .Then(new byte[] { 6, 7, 8, 9 })
                .Then((byte)LogEntryDataType.EndOfLog)
                .ToArray();

            FileHelper.CreateFile(this.path, fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            dataFile.Setup(d => d.WriteRaw(0, It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 1, 2, 3 })), 3));
            dataFile.Setup(d => d.WriteRaw(3, It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 4, 5 })), 2));
            dataFile.Setup(d => d.WriteRaw(5, It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 6, 7, 8, 9 })), 4));

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.RollbackDataTo(dataFile.Object);
            }

            dataFile.VerifyAll();
        }

        /// <summary>
        /// If the log state is None then an exception should be thrown if an attempt to rollback is made.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfLogIsNotOpen()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.None, 233)
                    .ToArray();

            FileHelper.CreateFile(this.path, fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            using (var logFileManager = new LogFileManager(this.path))
            {
                AssertRaisesException<PersistenceException>(() => logFileManager.RollbackDataTo(dataFile.Object), "Transaction log in an invalid state to rollback data - current state: None");
            }
        }

        /// <summary>
        /// If the log state is Committed then an exception should be thrown if an attempt to rollback is made.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfLogIsCommitted()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.TransactionCommitted, 233)
                    .ToArray();
            FileHelper.CreateFile(this.path, fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            using (var logFileManager = new LogFileManager(this.path))
            {
                AssertRaisesException<PersistenceException>(() => logFileManager.RollbackDataTo(dataFile.Object), "Transaction log in an invalid state to rollback data - current state: TransactionCommitted");
            }
        }

        /// <summary>
        /// If unexpected log entry type is encountered then an exception should be raised.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfUnexpectedLogEntryIsEncountered()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.TransactionLogged, 233)
                    .Then((byte)99)
                    .ToArray();
            FileHelper.CreateFile(this.path, fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            using (var logFileManager = new LogFileManager(this.path))
            {
                AssertRaisesException<PersistenceException>(() => logFileManager.RollbackDataTo(dataFile.Object), "Unexpected log entry in log file - the log file is possibly corrupted");
            }
        }

        /// <summary>
        /// Any data after an end of log marker should be ignored.
        /// </summary>
        [TestMethod]
        public void ShouldNotProcessDataAfterAnEndOfLogMarker()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.TransactionLogged, 233)
                    .Then((byte)LogEntryDataType.EndOfLog)
                    .Then((byte)99)
                    .ToArray();

            FileHelper.CreateFile(this.path, fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.RollbackDataTo(dataFile.Object);
            }
        }
    }
}

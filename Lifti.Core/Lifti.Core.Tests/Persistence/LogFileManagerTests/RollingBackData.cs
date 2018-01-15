// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.LogFileManagerTests
{
    #region Using statements

    using System;
    using System.Linq;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Moq;

    using NUnit.Framework;

    #endregion

    /// <summary>
    /// Tests for rolling data back to the data store from a log file.
    /// </summary>
    [TestFixture]
    public class RollingBackData : LogFileManagerTest
    {
        /// <summary>
        /// Any data after an end of log marker should be ignored.
        /// </summary>
        [Test]
        public void ShouldNotProcessDataAfterAnEndOfLogMarker()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.TransactionLogged, 233)
                    .Then((byte)LogEntryDataType.EndOfLog)
                    .Then((byte)99)
                    .ToArray();

            this.SetInitialData(fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            this.Sut.RollbackDataTo(dataFile.Object);
        }

        /// <summary>
        /// If the log state is Committed then an exception should be thrown if an attempt to rollback is made.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfLogIsCommitted()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.TransactionCommitted, 233)
                    .ToArray();

            this.SetInitialData(fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            Assert.Throws<PersistenceException>(() => this.Sut.RollbackDataTo(dataFile.Object), "Transaction log in an invalid state to rollback data - current state: TransactionCommitted");
        }

        /// <summary>
        /// If the log state is None then an exception should be thrown if an attempt to rollback is made.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfLogIsNotOpen()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.None, 233)
                    .ToArray();

            this.SetInitialData(fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            Assert.Throws<PersistenceException>(() => this.Sut.RollbackDataTo(dataFile.Object), "Transaction log in an invalid state to rollback data - current state: None");
        }

        /// <summary>
        /// If a null data file manager is provided, an exception should be thrown.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfNullDataFileManagerProvided()
        {
            Assert.Throws<ArgumentNullException>(() => this.Sut.RollbackDataTo(null), "Value cannot be null.\r\nParameter name: dataFileManager");
        }

        /// <summary>
        /// If unexpected log entry type is encountered then an exception should be raised.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfUnexpectedLogEntryIsEncountered()
        {
            var fileData = Data.LogFileHeader(TransactionLogState.TransactionLogged, 233)
                    .Then((byte)99)
                    .ToArray();

            this.SetInitialData(fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            Assert.Throws<PersistenceException>(() => this.Sut.RollbackDataTo(dataFile.Object), "Unexpected log entry in log file - the log file is possibly corrupted");
        }

        /// <summary>
        /// The state of the log file should be written to the log file.
        /// </summary>
        [Test]
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

            this.SetInitialData(fileData);

            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            dataFile.Setup(d => d.WriteRaw(0, It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 1, 2, 3 })), 3));
            dataFile.Setup(d => d.WriteRaw(3, It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 4, 5 })), 2));
            dataFile.Setup(d => d.WriteRaw(5, It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 6, 7, 8, 9 })), 4));

            this.Sut.RollbackDataTo(dataFile.Object);

            dataFile.VerifyAll();
        }
    }
}

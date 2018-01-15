// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.LogFileManagerTests
{
    #region Using statements

    using System;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Moq;

    using NUnit.Framework;

    using Should;

    #endregion

    /// <summary>
    /// Tests the logging data process for the <see cref="LogFileManager"/> class.
    /// </summary>
    [TestFixture]
    public class LoggingDataFromDataFile : LogFileManagerTest
    {
        /// <summary>
        /// If the log state is Committed then an exception should be thrown if the LogDataFrom method is called.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfLogIsCommitted()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.LogState = TransactionLogState.TransactionCommitted;
            Assert.Throws<PersistenceException>(
                () => this.Sut.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2),
                "Transaction log in an invalid state to log data - current state: TransactionCommitted");
        }

        /// <summary>
        /// If the log state is Logged then an exception should be thrown if the LogDataFrom method is called.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfLogIsLogged()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.LogState = TransactionLogState.TransactionLogged;
            Assert.Throws<PersistenceException>(
                () => this.Sut.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2),
                "Transaction log in an invalid state to log data - current state: TransactionLogged");
        }

        /// <summary>
        /// If the log state is None then an exception should be thrown if the LogDataFrom method is called.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfLogIsNotOpen()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            Assert.Throws<PersistenceException>(
                () => this.Sut.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2),
                "Transaction log in an invalid state to log data - current state: None");
        }

        /// <summary>
        /// If a null data file manager is provided, an exception should be thrown.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfNullDataFileManagerProvided()
        {
            Assert.Throws<ArgumentNullException>(() => this.Sut.LogDataFrom(LogEntryDataType.FullPage, null, 10, 200), "Value cannot be null.\r\nParameter name: dataFileManager");
        }

        /// <summary>
        /// When logging data from the data file, an entry type header should additionally be written out.
        /// </summary>
        [Test]
        public void ShouldWriteOutEntryHeaderAsWellAsData()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);
            dataFile.Setup(d => d.ReadRaw(10, It.Is<byte[]>(b => b.Length == 2))).Returns(new byte[] { 56, 22 });

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2);

            var expectedData = Data.LogFileHeader(TransactionLogState.Incomplete, 233)
                .Then(Data.LogEntryHeader(LogEntryDataType.PageHeader, 10, 2))
                .Then(new byte[] { 56, 22 });

            var actual = this.Stream.ToArray();

            actual.ShouldEqual(expectedData);
        }
    }
}

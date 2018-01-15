// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.LogFileManagerTests
{
    #region Using statements

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Moq;

    using NUnit.Framework;

    using Should;

    #endregion

    /// <summary>
    /// Tests the end of transaction process for the <see cref="LogFileManager"/> class.
    /// </summary>
    [TestFixture]
    public class EndingTransaction : LogFileManagerTest
    {
        /// <summary>
        /// The state of the log file should be marked as logged (but not committed) after ending logging.
        /// </summary>
        [Test]
        public void ShouldMarkTheStateAsLogged()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.EndLog();

            this.Sut.LogState.ShouldEqual(TransactionLogState.TransactionLogged);
        }

        /// <summary>
        /// If the log state is Committed then an exception should be thrown if the EndLog method is called.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfLogIsCommitted()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.LogState = TransactionLogState.TransactionCommitted;

            Assert.Throws<PersistenceException>(() => this.Sut.EndLog(), "Transaction log in an invalid state to end logging - current state: TransactionCommitted");
        }

        /// <summary>
        /// If the log state is Logged then an exception should be thrown if the EndLog method is called.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfLogIsLogged()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.LogState = TransactionLogState.TransactionLogged;

            Assert.Throws<PersistenceException>(() => this.Sut.EndLog(), "Transaction log in an invalid state to end logging - current state: TransactionLogged");
        }

        /// <summary>
        /// If the log state is None then an exception should be thrown if the EndLog method is called.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfLogIsNotOpen()
        {
            Assert.Throws<PersistenceException>(() => this.Sut.EndLog(), "Transaction log in an invalid state to end logging - current state: None");
        }

        /// <summary>
        /// When instructed, the end of log marker should be written out.
        /// </summary>
        [Test]
        public void ShouldWriteOutEndOfLogMarker()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);
            dataFile.Setup(d => d.ReadRaw(10, It.Is<byte[]>(b => b.Length == 2))).Returns((int i, byte[] b) => b);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2);
            this.Sut.EndLog();

            var expectedData = Data.LogFileHeader(TransactionLogState.TransactionLogged, 233)
                .Then(Data.LogEntryHeader(LogEntryDataType.PageHeader, 10, 2))
                .Then(new byte[] { 0, 0 })
                .Then((byte)LogEntryDataType.EndOfLog);

            var actual = this.Stream.ToArray();

            actual.ShouldEqual(expectedData);
        }
    }
}

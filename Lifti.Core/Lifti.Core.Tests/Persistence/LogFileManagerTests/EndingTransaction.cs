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
    /// Tests the end of transaction process for the <see cref="LogFileManager"/> class.
    /// </summary>
    [TestClass]
    public class EndingTransaction : UnitTestBase
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
        /// When instructed, the end of log marker should be written out.
        /// </summary>
        [TestMethod]
        public void ShouldWriteOutEndOfLogMarker()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);
            dataFile.Setup(d => d.ReadRaw(10, It.Is<byte[]>(b => b.Length == 2))).Returns((int i, byte[] b) => b);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
                logFileManager.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2);
                logFileManager.EndLog();
            }

            var expectedData = Data.LogFileHeader(TransactionLogState.TransactionLogged, 233)
                .Then(Data.LogEntryHeader(LogEntryDataType.PageHeader, 10, 2))
                .Then(new byte[] { 0, 0 })
                .Then((byte)LogEntryDataType.EndOfLog);

            var actual = FileHelper.GetFileBytes(this.path);

            Assert.IsTrue(expectedData.SequenceEqual(actual));
        }

        /// <summary>
        /// The state of the log file should be marked as logged (but not committed) after ending logging.
        /// </summary>
        [TestMethod]
        public void ShouldMarkTheStateAsLogged()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
                logFileManager.EndLog();

                Assert.AreEqual(TransactionLogState.TransactionLogged, logFileManager.LogState);
            }            
        }

        /// <summary>
        /// If the log state is None then an exception should be thrown if the EndLog method is called.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfLogIsNotOpen()
        {
            using (var logFileManager = new LogFileManager(this.path))
            {
                AssertRaisesException<PersistenceException>(() => logFileManager.EndLog(), "Transaction log in an invalid state to end logging - current state: None");
            }  
        }

        /// <summary>
        /// If the log state is Committed then an exception should be thrown if the EndLog method is called.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfLogIsCommitted()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
                logFileManager.LogState = TransactionLogState.TransactionCommitted;
                AssertRaisesException<PersistenceException>(() => logFileManager.EndLog(), "Transaction log in an invalid state to end logging - current state: TransactionCommitted");
            }
        }

        /// <summary>
        /// If the log state is Logged then an exception should be thrown if the EndLog method is called.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfLogIsLogged()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
                logFileManager.LogState = TransactionLogState.TransactionLogged;
                AssertRaisesException<PersistenceException>(() => logFileManager.EndLog(), "Transaction log in an invalid state to end logging - current state: TransactionLogged");
            }
        }
    }
}

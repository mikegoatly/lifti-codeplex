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
    /// Tests the logging data process for the <see cref="LogFileManager"/> class.
    /// </summary>
    [TestClass]
    public class LoggingDataFromDataFile : UnitTestBase
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
                AssertRaisesArgumentNullException(() => logFileManager.LogDataFrom(LogEntryDataType.FullPage, null, 10, 200), "dataFileManager");
            }
        }

        /// <summary>
        /// When logging data from the data file, an entry type header should additionally be written out.
        /// </summary>
        [TestMethod]
        public void ShouldWriteOutEntryHeaderAsWellAsData()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);
            dataFile.Setup(d => d.ReadRaw(10, It.Is<byte[]>(b => b.Length == 2))).Returns(new byte[] { 56, 22 });

            using (var logFileManager = new LogFileManager(this.path))
            {
                logFileManager.InitializeNewLog(dataFile.Object);
                logFileManager.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2);
            }

            var expectedData = Data.LogFileHeader(TransactionLogState.Incomplete, 233)
                .Then(Data.LogEntryHeader(LogEntryDataType.PageHeader, 10, 2))
                .Then(new byte[] { 56, 22 });

            var actual = FileHelper.GetFileBytes(this.path);

            Assert.IsTrue(expectedData.SequenceEqual(actual));
        }

        /// <summary>
        /// If the log state is None then an exception should be thrown if the LogDataFrom method is called.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfLogIsNotOpen()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);

            using (var logFileManager = new LogFileManager(this.path))
            {
                AssertRaisesException<PersistenceException>(
                    () => logFileManager.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2),
                    "Transaction log in an invalid state to log data - current state: None");
            }  
        }

        /// <summary>
        /// If the log state is Committed then an exception should be thrown if the LogDataFrom method is called.
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
                AssertRaisesException<PersistenceException>(
                    () => logFileManager.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2),
                    "Transaction log in an invalid state to log data - current state: TransactionCommitted");
            }
        }

        /// <summary>
        /// If the log state is Logged then an exception should be thrown if the LogDataFrom method is called.
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
                AssertRaisesException<PersistenceException>(
                    () => logFileManager.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 2),
                    "Transaction log in an invalid state to log data - current state: TransactionLogged");
            }
        }
    }
}

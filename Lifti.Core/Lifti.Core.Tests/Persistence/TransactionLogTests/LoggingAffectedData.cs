// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogTests
{
    using System.IO;
    using System.Linq;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the logging of affected data to the underlying log file.
    /// </summary>
    [TestClass]
    public class LoggingAffectedData : TransactionLogTestBase
    {
        /// <summary>
        /// If a page header is affected, only the page header data should be written out to the log file.
        /// </summary>
        [TestMethod]
        public void ShouldWriteAffectedPageHeaderDataToLog()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));
            logFileManager.Setup(l => l.LogDataFrom(LogEntryDataType.PageHeader, dataFileManager.Object, Data.HeaderSize + Data.PageManagerHeaderSize + (2 * Data.PageSize), Data.PageHeaderSize));
            logFileManager.Setup(l => l.EndLog());

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Header);

            log.LogExistingDataForAffectedPages();

            logFileManager.VerifyAll();
        }

        /// <summary>
        /// If a page body is affected, the entire page data should be written out to the log file.
        /// </summary>
        [TestMethod]
        public void ShouldWriteAffectedPageDataToLog()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            dataFileManager.Setup(m => m.GetDataReader(Data.HeaderSize + Data.PageManagerHeaderSize + (Data.PageSize * 3), Data.PageHeaderSize)).
                Returns(new BinaryReader(new MemoryStream(Data.IndexNodePage(1, 2, 5, 2, 4, 1234).ToArray())));

            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));
            logFileManager.Setup(l => l.LogDataFrom(LogEntryDataType.FullPage, dataFileManager.Object, Data.HeaderSize + Data.PageManagerHeaderSize + (3 * Data.PageSize), 1234));
            logFileManager.Setup(l => l.EndLog());

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(3);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Body);

            log.LogExistingDataForAffectedPages();

            logFileManager.VerifyAll();
        }

        /// <summary>
        /// If a page is created during a transaction, only the original header data should be logged.
        /// </summary>
        [TestMethod]
        public void ShouldOnlyLogHeaderOfPageIfItWasCreated()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));
            logFileManager.Setup(l => l.LogDataFrom(LogEntryDataType.PageHeader, dataFileManager.Object, Data.HeaderSize + Data.PageManagerHeaderSize + (3 * Data.PageSize), Data.PageHeaderSize));
            logFileManager.Setup(l => l.EndLog());

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(3);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Created);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Body);

            log.LogExistingDataForAffectedPages();

            logFileManager.VerifyAll();
        }

        /// <summary>
        /// If a page is created during a transaction in an extended part of the data file, nothing needs to be logged.
        /// </summary>
        [TestMethod]
        public void ShouldLogNothingIfPageWasCreatedAfterTheOriginalExtentOfTheDataFile()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));
            logFileManager.Setup(l => l.EndLog());

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(8);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Created);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Body);

            log.LogExistingDataForAffectedPages();

            logFileManager.VerifyAll();
        }

        /// <summary>
        /// Should not write any page data if none is affectd..
        /// </summary>
        [TestMethod]
        public void ShouldNotWritePageDataToLogIfNoneAffected()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));
            logFileManager.Setup(l => l.EndLog());

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            log.LogExistingDataForAffectedPages();

            logFileManager.VerifyAll();
        }

        /// <summary>
        /// If the page manager header data if affected, then the original page manager header data should be written out to the log.
        /// </summary>
        [TestMethod]
        public void ShouldWritePageManagerHeaderDataIfAffected()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));
            logFileManager.Setup(l => l.LogDataFrom(LogEntryDataType.PageManagerHeader, dataFileManager.Object, Data.HeaderSize, Data.PageManagerHeaderSize));
            logFileManager.Setup(l => l.EndLog());

            var log = CreateTransactionLog(dataFileManager, logFileManager);
            log.PageManagerHeaderWritten = true;

            log.LogExistingDataForAffectedPages();

            logFileManager.VerifyAll();
        }

        /// <summary>
        /// If the transaction log has already written to file once, then any subsequent attempts to log again
        /// should fail.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfTransactionLogAlreadyWrittenToFileOnce()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            log.LogExistingDataForAffectedPages();

            AssertRaisesException<PersistenceException>(() => log.LogExistingDataForAffectedPages(), "Transaction log has already been written. Unable to log further information.");
        }

        /// <summary>
        /// Once the existing data has been written to the log file, the TransactionComplete property should
        /// be set to true.
        /// </summary>
        [TestMethod]
        public void ShouldMarkTransactionAsCompletedOnceLogged()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            log.LogExistingDataForAffectedPages();

            Assert.IsTrue(log.TransactionComplete);
        }
    }
}

// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogFactoryTests
{
    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for creating a new <see cref="TransactionLog"/> instance using a <see cref="TransactionLogFactory"/>.
    /// </summary>
    [TestClass]
    public class CreatingNewTransactionLog
    {
        /// <summary>
        /// The factory should pass a created transaction log instance the current total page count
        /// and data file manager from the provided page manager.
        /// </summary>
        [TestMethod]
        public void ShouldPassTheOriginalPageCount()
        {
            var logFile = new Mock<ILogFileManager>();
            var dataFile = new Mock<IDataFileManager>();
            var factory = new TransactionLogFactory(logFile.Object);

            var pageManager = new Mock<IPageManager>();
            pageManager.SetupGet(p => p.TotalPageCount).Returns(9);
            pageManager.SetupGet(p => p.FileManager).Returns(dataFile.Object);

            var transactionLog = factory.CreateTransactionLog("1", pageManager.Object);
            Assert.IsNotNull(transactionLog);

            Assert.AreEqual(9, transactionLog.OriginalPageCount);

            pageManager.VerifyAll();
        }

        /// <summary>
        /// The construction of a new transaction log should cause the log file
        /// to have a new transaction initialized in it.
        /// </summary>
        [TestMethod]
        public void ShouldCauseTheLogFileToInitializeANewTransaction()
        {
            var logFile = new Mock<ILogFileManager>(MockBehavior.Strict);
            var dataFile = new Mock<IDataFileManager>();
            var factory = new TransactionLogFactory(logFile.Object);

            logFile.Setup(l => l.InitializeNewLog(dataFile.Object));

            var pageManager = new Mock<IPageManager>();
            pageManager.SetupGet(p => p.TotalPageCount).Returns(9);
            pageManager.SetupGet(p => p.FileManager).Returns(dataFile.Object);

            var transactionLog = factory.CreateTransactionLog("1", pageManager.Object);

            logFile.VerifyAll();
            pageManager.VerifyAll();
        }

        /// <summary>
        /// The construction of a new transaction log should cause the log file
        /// to be set up with the provided transaction id.
        /// </summary>
        [TestMethod]
        public void ShouldConfigureTheTransactionLogWithTheCorrectTransactionId()
        {
            var logFile = new Mock<ILogFileManager>(MockBehavior.Strict);
            var dataFile = new Mock<IDataFileManager>();
            var factory = new TransactionLogFactory(logFile.Object);

            logFile.Setup(l => l.InitializeNewLog(dataFile.Object));

            var pageManager = new Mock<IPageManager>();
            pageManager.SetupGet(p => p.TotalPageCount).Returns(9);
            pageManager.SetupGet(p => p.FileManager).Returns(dataFile.Object);

            var transactionLog = factory.CreateTransactionLog("1", pageManager.Object);

            Assert.AreEqual("1", transactionLog.TransactionId);

            logFile.VerifyAll();
            pageManager.VerifyAll();
        }
    }
}

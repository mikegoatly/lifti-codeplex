// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogTests
{
    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests getting pages affected during a transaction from the transaction log.
    /// </summary>
    [TestClass]
    public class GettingPagesAffectedByTransaction : TransactionLogTestBase
    {
        /// <summary>
        /// If a page is affected by a header write, it should be returned by the transaction log
        /// cache.
        /// </summary>
        [TestMethod]
        public void ShouldReturnPageAffectedByHeaderWrite()
        {
            var dataFileManager = new Mock<IDataFileManager>(MockBehavior.Strict);
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Header);

            Assert.AreSame(dataPage.Object, log.TryGetPage(2));
        }

        /// <summary>
        /// If a page is affected by a body write, it should be returned by the transaction log
        /// cache.
        /// </summary>
        [TestMethod]
        public void ShouldReturnPageAffectedByBodyWrite()
        {
            var dataFileManager = new Mock<IDataFileManager>(MockBehavior.Strict);
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Body);

            Assert.AreSame(dataPage.Object, log.TryGetPage(2));
        }

        /// <summary>
        /// If a page is created, it should be returned by the transaction log
        /// cache.
        /// </summary>
        [TestMethod]
        public void ShouldReturnPageAffectedByCreation()
        {
            var dataFileManager = new Mock<IDataFileManager>(MockBehavior.Strict);
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(l => l.InitializeNewLog(dataFileManager.Object));

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Created);

            Assert.AreSame(dataPage.Object, log.TryGetPage(2));
        }
    }
}

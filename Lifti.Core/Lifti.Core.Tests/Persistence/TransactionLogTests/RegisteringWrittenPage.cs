// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogTests
{
    using System.Linq;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the process of registering that a page (or just its header) has been
    /// affected during a transaction.
    /// </summary>
    [TestFixture]
    public class RegisteringWrittenPage : TransactionLogTestBase
    {
        /// <summary>
        /// If just the page header is touched, then it should be recorded as such,
        /// and no page bodies should be affected.
        /// </summary>
        [Test]
        public void ShouldRecordAHeaderBeingWritten()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Header);

            Assert.AreEqual(1, log.AffectedPageHeaders.Count());
            Assert.AreEqual(0, log.AffectedPageBodies.Count());
            Assert.AreEqual(0, log.CreatedPageNumbers.Count());
            Assert.AreSame(dataPage.Object, log.AffectedPageHeaders.Single());
        }

        /// <summary>
        /// If the page body is touched, then it should be recorded as such,
        /// and should not be reported as a page header being affected.
        /// </summary>
        [Test]
        public void ShouldRecordABodyBeingWritten()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Body);

            Assert.AreEqual(0, log.AffectedPageHeaders.Count());
            Assert.AreEqual(1, log.AffectedPageBodies.Count());
            Assert.AreEqual(0, log.CreatedPageNumbers.Count());
            Assert.AreSame(dataPage.Object, log.AffectedPageBodies.Single());
        }

        /// <summary>
        /// If just the page header is touched, then the page body is affected,
        /// the full page and headers being changed should be reported.
        /// </summary>
        [Test]
        public void PageBodyWriteShouldNotOverwritePageHeaderBeingWritten()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Header);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Body);

            Assert.AreEqual(1, log.AffectedPageHeaders.Count());
            Assert.AreEqual(1, log.AffectedPageBodies.Count());
            Assert.AreEqual(0, log.CreatedPageNumbers.Count());
            Assert.AreSame(dataPage.Object, log.AffectedPageHeaders.Single());
            Assert.AreSame(dataPage.Object, log.AffectedPageBodies.Single());
        }

        /// <summary>
        /// If the page is reported as created, having its header and body
        /// modified should be reported.
        /// </summary>
        [Test]
        public void PageBodyWriteShouldNotOverwritePageHeaderBeingWrittenOrPageCreation()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage = new Mock<IDataPage>();
            dataPage.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Created);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Header);
            log.RegisterAffectedPage(dataPage.Object, PageWriteLevels.Body);

            Assert.AreEqual(1, log.AffectedPageHeaders.Count());
            Assert.AreEqual(1, log.AffectedPageBodies.Count());
            Assert.AreEqual(1, log.CreatedPageNumbers.Count());
            Assert.AreEqual(2, log.CreatedPageNumbers.First());
            Assert.AreSame(dataPage.Object, log.AffectedPageHeaders.Single());
            Assert.AreSame(dataPage.Object, log.AffectedPageBodies.Single());
        }
    }
}

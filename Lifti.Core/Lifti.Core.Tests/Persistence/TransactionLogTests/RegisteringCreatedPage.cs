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
    /// Test for when a new page is created during a transaction.
    /// </summary>
    [TestFixture]
    public class RegisteringCreatedPage : TransactionLogTestBase
    {
        /// <summary>
        /// When a page is registered as being created, it should be reported
        /// as such when requested.
        /// </summary>
        [Test]
        public void ShouldRecordPageAsBeingCreated()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            var dataPage1 = new Mock<IDataPage>();
            var dataPage2 = new Mock<IDataPage>();
            dataPage1.SetupGet(p => p.Header.PageNumber).Returns(1);
            dataPage2.SetupGet(p => p.Header.PageNumber).Returns(2);
            log.RegisterAffectedPage(dataPage1.Object, PageWriteLevels.Created);
            log.RegisterAffectedPage(dataPage2.Object, PageWriteLevels.Created);

            Assert.AreEqual(0, log.AffectedPageHeaders.Count());
            Assert.AreEqual(0, log.AffectedPageBodies.Count());
            Assert.AreEqual(2, log.CreatedPageNumbers.Count());
            Assert.AreEqual(1, log.CreatedPageNumbers.First());
            Assert.AreEqual(2, log.CreatedPageNumbers.Skip(1).First());
        }
    }
}

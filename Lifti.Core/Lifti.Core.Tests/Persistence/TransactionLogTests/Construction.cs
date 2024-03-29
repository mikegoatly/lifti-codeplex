﻿// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogTests
{
    using System.Linq;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the construction of a <see cref="TransactionLog"/> instance.
    /// </summary>
    [TestFixture]
    public class Construction : TransactionLogTestBase
    {
        /// <summary>
        /// If a null log file manager is provided, then a <see cref="ArgumentNullException"/> should be thrown.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfNullLogFileManagerProvided()
        {
            var dataFileManager = new Mock<IDataFileManager>();

            this.AssertRaisesArgumentNullException(() => new TransactionLog("1", 8, null, dataFileManager.Object), "logFileManager");
        }

        /// <summary>
        /// If a null data file manager is provided, then a <see cref="ArgumentNullException"/> should be thrown.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfNullDataFileManagerProvided()
        {
            var logFileManager = new Mock<ILogFileManager>();

            this.AssertRaisesArgumentNullException(() => new TransactionLog("1", 8, logFileManager.Object, null), "dataFileManager");
        }

        /// <summary>
        /// The transaction log should maintain the original number of pages contained in the page manager
        /// when the transaction started.
        /// </summary>
        [Test]
        public void ShouldStoreOriginalPageCount()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            Assert.AreEqual(8, log.OriginalPageCount);
        }

        /// <summary>
        /// When constructing, the log transaction should call through to the underlying data
        /// store to indicate a new transaction has started.
        /// </summary>
        [Test]
        public void ShouldInitializeANewTransactionInTheLogFileManager()
        {
            var dataFileManager = new Mock<IDataFileManager>(MockBehavior.Strict);
            var logFileManager = new Mock<ILogFileManager>(MockBehavior.Strict);
            logFileManager.Setup(m => m.InitializeNewLog(dataFileManager.Object));

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            logFileManager.Verify(m => m.InitializeNewLog(dataFileManager.Object), Times.Exactly(1));
        }

        /// <summary>
        /// The transaction complete flag should default to false.
        /// </summary>
        [Test]
        public void ShouldDefaultTransactionCompleteToFalse()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            Assert.IsFalse(log.TransactionComplete);
        }

        /// <summary>
        /// The page manager header written flag should default to false.
        /// </summary>
        [Test]
        public void ShouldDefaultPageManagerHeaderWrittenToFalse()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            Assert.IsFalse(log.PageManagerHeaderWritten);
        }

        /// <summary>
        /// The created pages set should be empty by default.
        /// </summary>
        [Test]
        public void CreatedPagesShouldBeEmptyByDefault()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            Assert.AreEqual(0, log.CreatedPageNumbers.Count());
        }

        /// <summary>
        /// The affected page bodies set should be empty by default.
        /// </summary>
        [Test]
        public void AffectedPageBodiesShouldBeEmptyByDefault()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            Assert.AreEqual(0, log.AffectedPageBodies.Count());
        }

        /// <summary>
        /// The affected page headers set should be empty by default.
        /// </summary>
        [Test]
        public void AffectedPageHeadersShouldBeEmptyByDefault()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            Assert.AreEqual(0, log.AffectedPageHeaders.Count());
        }
    }
}

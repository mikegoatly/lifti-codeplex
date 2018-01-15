// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogRollbackTests
{

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the construction of a <see cref="TransactionLogRollback"/> instance.
    /// </summary>
    [TestFixture]
    public class Construction
    {
        /// <summary>
        /// If a null log file manager is provided, then a <see cref="ArgumentNullException"/> should be thrown.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfNullLogFileManagerProvided()
        {
            var dataFileManager = new Mock<IDataFileManager>();

            this.AssertRaisesArgumentNullException(() => new TransactionLogRollback(null, dataFileManager.Object), "logFileManager");
        }

        /// <summary>
        /// If a null data file manager is provided, then a <see cref="ArgumentNullException"/> should be thrown.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfNullDataFileManagerProvided()
        {
            var logFileManager = new Mock<ILogFileManager>();

            this.AssertRaisesArgumentNullException(() => new TransactionLogRollback(logFileManager.Object, null), "dataFileManager");
        }
    }
}

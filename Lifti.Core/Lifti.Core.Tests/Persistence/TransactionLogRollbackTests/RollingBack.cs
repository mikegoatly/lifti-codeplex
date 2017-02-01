// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogRollbackTests
{
    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for using a <see cref="TransactionLogRollback"/> instance to rollback an existing
    /// logged transaction.
    /// </summary>
    [TestClass]
    public class RollingBack
    {
        /// <summary>
        /// If the current transaction log state is None, then rolling back should do nothing.
        /// </summary>
        [TestMethod]
        public void ShouldDoNothingIfCurrentStateIsNone()
        {
            var log = new Mock<ILogFileManager>(MockBehavior.Strict);
            var data = new Mock<IDataFileManager>(MockBehavior.Strict);

            log.SetupGet(l => l.LogState).Returns(TransactionLogState.None);

            var rollback = new TransactionLogRollback(log.Object, data.Object);

            rollback.Rollback();

            log.VerifyAll();
            data.VerifyAll();
        }

        /// <summary>
        /// If the current transaction log state is Committed, then rolling back should do nothing.
        /// </summary>
        [TestMethod]
        public void ShouldDoNothingIfCurrentStateIsCommitted()
        {
            var log = new Mock<ILogFileManager>(MockBehavior.Strict);
            var data = new Mock<IDataFileManager>(MockBehavior.Strict);

            log.SetupGet(l => l.LogState).Returns(TransactionLogState.TransactionCommitted);

            var rollback = new TransactionLogRollback(log.Object, data.Object);

            rollback.Rollback();

            log.VerifyAll();
            data.VerifyAll();
        }

        /// <summary>
        /// Only the extent of the data file should be rolled back if transaction not fully written.
        /// </summary>
        [TestMethod]
        public void ShouldOnlyRollbackExtentIfTransactionNotFullyWritten()
        {
            var log = new Mock<ILogFileManager>(MockBehavior.Strict);
            var data = new Mock<IDataFileManager>(MockBehavior.Strict);

            log.SetupGet(l => l.LogState).Returns(TransactionLogState.Incomplete);
            log.SetupGet(l => l.OriginalDataFileExtent).Returns(200);
            log.SetupSet(l => l.LogState = TransactionLogState.None);
            data.SetupGet(d => d.CurrentLength).Returns(400);
            data.Setup(d => d.ShrinkStream(200));

            var rollback = new TransactionLogRollback(log.Object, data.Object);

            rollback.Rollback();

            log.VerifyAll();
            data.VerifyAll();
        }

        /// <summary>
        /// If the extent of the data file isn't changed its extent shouldn't be affected.
        /// </summary>
        [TestMethod]
        public void ShouldNotRollbackExtentIfLengthUnchanged()
        {
            var log = new Mock<ILogFileManager>(MockBehavior.Strict);
            var data = new Mock<IDataFileManager>(MockBehavior.Strict);

            log.SetupGet(l => l.LogState).Returns(TransactionLogState.Incomplete);
            log.SetupGet(l => l.OriginalDataFileExtent).Returns(200);
            log.SetupSet(l => l.LogState = TransactionLogState.None);
            data.SetupGet(d => d.CurrentLength).Returns(200);

            var rollback = new TransactionLogRollback(log.Object, data.Object);

            rollback.Rollback();

            log.VerifyAll();
            data.VerifyAll();
        }

        /// <summary>
        /// If the transaction is logged but not fully committed, it should be fully rolled back.
        /// </summary>
        [TestMethod]
        public void ShouldRollbackDataAndShrinkExtentIfLoggedAndTransactionFullyWritten()
        {
            var log = new Mock<ILogFileManager>(MockBehavior.Strict);
            var data = new Mock<IDataFileManager>(MockBehavior.Strict);

            log.SetupGet(l => l.LogState).Returns(TransactionLogState.TransactionLogged);
            log.SetupGet(l => l.OriginalDataFileExtent).Returns(200);
            log.SetupSet(l => l.LogState = TransactionLogState.None);
            log.Setup(l => l.RollbackDataTo(data.Object));
            data.SetupGet(d => d.CurrentLength).Returns(400);
            data.Setup(d => d.ShrinkStream(200));

            var rollback = new TransactionLogRollback(log.Object, data.Object);

            rollback.Rollback();

            log.VerifyAll();
            data.VerifyAll();
        }
    }
}

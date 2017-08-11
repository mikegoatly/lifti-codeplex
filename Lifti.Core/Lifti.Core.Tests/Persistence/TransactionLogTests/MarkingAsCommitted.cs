// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogTests
{
    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for setting the transaction log state to committed.
    /// </summary>
    [TestFixture]
    public class MarkingAsCommitted : TransactionLogTestBase
    {
        /// <summary>
        /// Marking the transaction log should just call through to the log file manager, setting the
        /// log state to TransactionCommitted..
        /// </summary>
        [Test]
        public void ShouldSetTheLogFileStateToCommitted()
        {
            var dataFileManager = new Mock<IDataFileManager>();
            var logFileManager = new Mock<ILogFileManager>();

            var log = CreateTransactionLog(dataFileManager, logFileManager);

            log.MarkAsCommitted();

            logFileManager.VerifySet(l => l.LogState = TransactionLogState.TransactionCommitted, Times.Exactly(1));
        }
    }
}

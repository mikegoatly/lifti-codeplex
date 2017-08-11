// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogTests
{
    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Moq;

    /// <summary>
    /// The base test class for TransactionLog tests.
    /// </summary>
    public abstract class TransactionLogTestBase
    {
        /// <summary>
        /// Creates a transaction log.
        /// </summary>
        /// <param name="dataFileManager">The data file manager.</param>
        /// <param name="logFileManager">The log file manager.</param>
        /// <returns>The created transaction log.</returns>
        protected static TransactionLog CreateTransactionLog(Mock<IDataFileManager> dataFileManager, Mock<ILogFileManager> logFileManager)
        {
            return new TransactionLog("1", 8, logFileManager.Object, dataFileManager.Object);
        }
    }
}

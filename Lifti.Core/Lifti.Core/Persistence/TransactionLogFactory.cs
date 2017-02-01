// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;

    using Lifti.Persistence.IO;

    /// <summary>
    /// A factory class capable of creating an <see cref="ITransactionLog"/> instance for
    /// a full text index transaction.
    /// </summary>
    public class TransactionLogFactory : ITransactionLogFactory
    {
        /// <summary>
        /// The underlying log file manager.
        /// </summary>
        private ILogFileManager logFileManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogFactory"/> class.
        /// </summary>
        /// <param name="logFileManager">The underlying log file manager.</param>
        public TransactionLogFactory(ILogFileManager logFileManager)
        {
            this.logFileManager = logFileManager;
        }

        /// <summary>
        /// Creates a transaction log instance for a new transaction.
        /// </summary>
        /// <param name="transactionId">The unique transaction id of the transaction associated to the log.</param>
        /// <param name="pageManager">The page manager to create the transaction log for.</param>
        /// <returns>
        /// The created <see cref="ITransactionLog"/> instance.
        /// </returns>
        public ITransactionLog CreateTransactionLog(string transactionId, IPageManager pageManager)
        {
            if (pageManager == null)
            {
                throw new ArgumentNullException(nameof(pageManager));
            }

            return new TransactionLog(transactionId, pageManager.TotalPageCount, this.logFileManager, pageManager.FileManager);
        }

        /// <summary>
        /// Creates a transaction log rollback instance.
        /// </summary>
        /// <param name="pageManager">The page manager to create the transaction log rollback for.</param>
        /// <returns>
        /// The created <see cref="ITransactionLogRollback"/> instance.
        /// </returns>
        public ITransactionLogRollback CreateTransactionLogRollback(IPageManager pageManager)
        {
            if (pageManager == null)
            {
                throw new ArgumentNullException(nameof(pageManager));
            }

            return new TransactionLogRollback(this.logFileManager, pageManager.FileManager);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.logFileManager != null)
                {
                    this.logFileManager.Dispose();
                    this.logFileManager = null;
                }
            }
        }
    }
}

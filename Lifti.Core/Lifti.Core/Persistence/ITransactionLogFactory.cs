// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;

    /// <summary>
    /// The interface implemented by factory classes capable of creating <see cref="ITransactionLog"/> and
    /// <see cref="ITransactionLogRollback"/> instances.
    /// </summary>
    public interface ITransactionLogFactory : IDisposable
    {
        /// <summary>
        /// Creates a transaction log instance for a new transaction.
        /// </summary>
        /// <param name="transactionId">The unique transaction id of the transaction associated to the log.</param>
        /// <param name="pageManager">The page manager to create the transaction log for.</param>
        /// <returns>
        /// The created <see cref="ITransactionLog"/> instance.
        /// </returns>
        ITransactionLog CreateTransactionLog(string transactionId, IPageManager pageManager);

        /// <summary>
        /// Creates a transaction log rollback instance.
        /// </summary>
        /// <param name="pageManager">The page manager to create the transaction log rollback for.</param>
        /// <returns>
        /// The created <see cref="ITransactionLogRollback"/> instance.
        /// </returns>
        ITransactionLogRollback CreateTransactionLogRollback(IPageManager pageManager);
    }
}
// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    /// <summary>
    /// The interface implemented by classes that can rollback a transaction from a log file.
    /// </summary>
    public interface ITransactionLogRollback
    {
        /// <summary>
        /// Rollbacks the transaction stored in the log file.
        /// </summary>
        void Rollback();
    }
}
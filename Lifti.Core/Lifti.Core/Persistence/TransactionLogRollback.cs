// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;

    using Lifti.Persistence.IO;

    /// <summary>
    /// A class capable of handling the logic of rolling back a transaction, in whatever state
    /// it was left in.
    /// </summary>
    public class TransactionLogRollback : ITransactionLogRollback
    {
        /// <summary>
        /// The file manager for the underlying data file.
        /// </summary>
        private readonly IDataFileManager dataFileManager;

        /// <summary>
        /// The file manager for the underlying log file.
        /// </summary>
        private readonly ILogFileManager logFileManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogRollback"/> class.
        /// </summary>
        /// <param name="logFileManager">The file manager for the underlying log file.</param>
        /// <param name="dataFileManager">The file manager for the underlying data file.</param>
        public TransactionLogRollback(ILogFileManager logFileManager, IDataFileManager dataFileManager)
        {
            this.logFileManager = logFileManager ?? throw new ArgumentNullException(nameof(logFileManager));
            this.dataFileManager = dataFileManager ?? throw new ArgumentNullException(nameof(dataFileManager));
        }

        /// <summary>
        /// Rollbacks the transaction stored in the log file.
        /// </summary>
        public void Rollback()
        {
            switch (this.logFileManager.LogState)
            {
                case TransactionLogState.Incomplete:
                    // The transaction wasn't fully written to the log, so the worst that can have happened
                    // is that the data file's extent was modified.
                    this.RollbackExtent();

                    // Mark the log as voided
                    this.logFileManager.LogState = TransactionLogState.None;
                    break;

                case TransactionLogState.TransactionLogged:
                    // The whole log was written - it must be completely rolled back, including any extent changes.
                    this.RollbackData();
                    this.RollbackExtent();

                    // Mark the log as voided
                    this.logFileManager.LogState = TransactionLogState.None;
                    break;
            }
        }

        /// <summary>
        /// Rollbacks the data from the log to the IO manager.
        /// </summary>
        /// <remarks>There is no longer any need to do safe reads because the log state
        /// indicates that the log was completely written.</remarks>
        private void RollbackData()
        {
            this.logFileManager.RollbackDataTo(this.dataFileManager);
        }

        /// <summary>
        /// Rolls back the extent of the data file. This occurs when the data file was extended during a transaction
        /// that has subsequently been rolled back.
        /// </summary>
        private void RollbackExtent()
        {
            var originalExtent = this.logFileManager.OriginalDataFileExtent;

            if (originalExtent < this.dataFileManager.CurrentLength)
            {
                this.dataFileManager.ShrinkStream(originalExtent);
            }
        }
    }
}

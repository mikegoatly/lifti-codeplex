// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Collections.Generic;

    /// <summary>
    /// The interface implemented by classes that can keep track of data that has been
    /// affected during a transaction.
    /// </summary>
    public interface ITransactionLog
    {
        /// <summary>
        /// Gets the page that only had their headers affected during the transaction.
        /// </summary>
        IEnumerable<IDataPage> AffectedPageHeaders { get; }

        /// <summary>
        /// Gets the pages that had their entire contents affected during the transaction.
        /// </summary>
        IEnumerable<IDataPage> AffectedPageBodies { get; }

        /// <summary>
        /// Gets the pages that were affected during the transaction in some form or another.
        /// </summary>
        IEnumerable<IDataPage> AffectedPages { get; }

        /// <summary>
        /// Gets the page numbers of the pages that were created during the transaction.
        /// </summary>
        /// <value>The created pages.</value>
        IEnumerable<int> CreatedPageNumbers { get; }

        /// <summary>
        /// Gets a value indicating whether the transaction log is closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the transaction log is complete and closed; otherwise, <c>false</c>.
        /// </value>
        bool TransactionComplete { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the page manager header was written during the transaction.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the page manager header was written during the transaction; otherwise, <c>false</c>.
        /// </value>
        bool PageManagerHeaderWritten { get; set; }

        /// <summary>
        /// Gets the unique transaction id of the transaction associated to the log.
        /// </summary>
        /// <value>The unique transaction ID.</value>
        string TransactionId { get; }

        /// <summary>
        /// Gets the total page count at the start of the transaction.
        /// </summary>
        /// <value>The original total page count.</value>
        int OriginalPageCount { get; }

        /// <summary>
        /// Registers that a page was affected out during the transaction.
        /// </summary>
        /// <param name="page">The page that was written.</param>
        /// <param name="writtenLevel">The degree to which the page was written out.</param>
        void RegisterAffectedPage(IDataPage page, PageWriteLevels writtenLevel);

        /// <summary>
        /// Tries to get the page for the given page number. This will only return the page
        /// if it has already been affected by the transaction.
        /// </summary>
        /// <param name="pageNumber">The page number of the page to try to get.</param>
        /// <returns>The <see cref="IDataPage"/> instance if the page has already been affected
        /// by the transaction, otherwise null.</returns>
        IDataPage TryGetPage(int pageNumber);

        /// <summary>
        /// Marks the transaction log as as committed.
        /// </summary>
        void MarkAsCommitted();

        /// <summary>
        /// Logs the original data from the affected pages into the log file.
        /// </summary>
        void LogExistingDataForAffectedPages();
    }
}
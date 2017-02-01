// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Lifti.Persistence.IO;

    /// <summary>
    /// The degree to which a page was written during a transaction.
    /// </summary>
    [Flags]
    public enum PageWriteLevels
    {
        /// <summary>
        /// The page was not written out.
        /// </summary>
        None = 0,

        /// <summary>
        /// The page has been created.
        /// </summary>
        Created = 1,

        /// <summary>
        /// The page header was written during the transaction.
        /// </summary>
        Header = 2,

        /// <summary>
        /// The page body was written during the transaction.
        /// </summary>
        Body = 4
    }

    /// <summary>
    /// The various states of a transaction log.
    /// </summary>
    public enum TransactionLogState
    {
        /// <summary>
        /// No transaction log has been written.
        /// </summary>
        None,

        /// <summary>
        /// The transaction log is not complete and the underlying IO manager is unaffected,
        /// although the extent of the file may have been modified.
        /// </summary>
        Incomplete,

        /// <summary>
        /// The transaction log has been completely written and the updates have been/are being written to the
        /// IO manager.
        /// </summary>
        TransactionLogged,

        /// <summary>
        /// The transaction has been fully committed and the log file can be re-used.
        /// </summary>
        TransactionCommitted
    }

    /// <summary>
    /// The various types of logged data sections in the log file.
    /// </summary>
    public enum LogEntryDataType
    {
        /// <summary>
        /// The following bytes are the page manager header bytes.
        /// </summary>
        PageManagerHeader,

        /// <summary>
        /// The following bytes are the bytes from a page header.
        /// </summary>
        PageHeader,

        /// <summary>
        /// The following bytes are the bytes from a page.
        /// </summary>
        FullPage,

        /// <summary>
        /// The end of the transaction log has been reached.
        /// </summary>
        EndOfLog
    }

    /// <summary>
    /// A log of actions performed during a transaction.
    /// </summary>
    public class TransactionLog : ITransactionLog
    {
        /// <summary>
        /// The data file manager.
        /// </summary>
        private readonly IDataFileManager dataFileManager;

        /// <summary>
        /// The file manager for the underlying log file.
        /// </summary>
        private readonly ILogFileManager logFileManager;

        /// <summary>
        /// Information about the pages and the degree to which they were written out during the transaction.
        /// </summary>
        private readonly Dictionary<int, PageWriteLevels> pageWriteLevels = new Dictionary<int, PageWriteLevels>();

        /// <summary>
        /// The cache of pages that have been written to during the transaction.
        /// </summary>
        private readonly Dictionary<int, IDataPage> writtenPages = new Dictionary<int, IDataPage>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLog"/> class.
        /// </summary>
        /// <param name="transactionId">The unique transaction id of the transaction associated to the log.</param>
        /// <param name="originalPageCount">The original page count.</param>
        /// <param name="logFileManager">The log file manager.</param>
        /// <param name="dataFileManager">The data file manager.</param>
        public TransactionLog(string transactionId, int originalPageCount, ILogFileManager logFileManager, IDataFileManager dataFileManager)
        {
            if (transactionId == null)
            {
                throw new ArgumentNullException(nameof(transactionId));
            }

            if (logFileManager == null)
            {
                throw new ArgumentNullException(nameof(logFileManager));
            }

            if (dataFileManager == null)
            {
                throw new ArgumentNullException(nameof(dataFileManager));
            }

            this.TransactionId = transactionId;
            this.OriginalPageCount = originalPageCount;
            this.logFileManager = logFileManager;
            this.dataFileManager = dataFileManager;

            this.logFileManager.InitializeNewLog(dataFileManager);
        }

        /// <summary>
        /// Gets the page that only had their headers affected during the transaction.
        /// </summary>
        public IEnumerable<IDataPage> AffectedPageHeaders
        {
            get
            {
                return this.GetPagesWithWriteFlag(PageWriteLevels.Header);
            }
        }

        /// <summary>
        /// Gets the pages that had their entire contents affected during the transaction.
        /// </summary>
        public IEnumerable<IDataPage> AffectedPageBodies
        {
            get
            {
                return this.GetPagesWithWriteFlag(PageWriteLevels.Body);
            }
        }

        /// <summary>
        /// Gets the pages that were affected during the transaction in some form or another.
        /// </summary>
        public IEnumerable<IDataPage> AffectedPages
        {
            get
            {
                return this.pageWriteLevels.Keys.Select(p => this.writtenPages[p]);
            }
        }

        /// <summary>
        /// Gets the pages that were created during the transaction.
        /// </summary>
        /// <value>The created pages.</value>
        public IEnumerable<int> CreatedPageNumbers
        {
            get
            {
                return this.GetPagesNumbersWithWriteFlag(PageWriteLevels.Created);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the transaction log is closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the transaction log is complete and closed; otherwise, <c>false</c>.
        /// </value>
        public bool TransactionComplete
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total page count at the start of the transaction.
        /// </summary>
        /// <value>The original total page count.</value>
        public int OriginalPageCount
        {
            get; }

        /// <summary>
        /// Gets or sets a value indicating whether the page manager header was written during the transaction.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the page manager header was written during the transaction; otherwise, <c>false</c>.
        /// </value>
        public bool PageManagerHeaderWritten
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the unique transaction id of the transaction associated to the log.
        /// </summary>
        /// <value>The unique transaction ID.</value>
        public string TransactionId
        {
            get; }

        /// <summary>
        /// Registers that a page was affected out during the transaction.
        /// </summary>
        /// <param name="page">The page that was written.</param>
        /// <param name="writtenLevel">The degree to which the page was written out.</param>
        public virtual void RegisterAffectedPage(IDataPage page, PageWriteLevels writtenLevel)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            PageWriteLevels existingWriteLevel;
            var pageNumber = page.Header.PageNumber;
            if (this.pageWriteLevels.TryGetValue(pageNumber, out existingWriteLevel))
            {
                // Only update the write level for the page if the new level is greater than the existing one
                if ((writtenLevel & existingWriteLevel) != writtenLevel)
                {
                    this.pageWriteLevels[pageNumber] = existingWriteLevel | writtenLevel;
                }

                // When a page is invalidated and then re-allocated, the body instance changes.
                // To this end we need to make sure that the written page reference is up to date.
                this.writtenPages[pageNumber] = page;
            }
            else
            {
                // Store the written page and the level it has currently been written at
                this.pageWriteLevels.Add(pageNumber, writtenLevel);
                this.writtenPages.Add(pageNumber, page);
            }
        }

        /// <summary>
        /// Tries to get the page for the given page number. This will only return the page
        /// if it has already been affected by the transaction.
        /// </summary>
        /// <param name="pageNumber">The page number of the page to try to get.</param>
        /// <returns>The <see cref="IDataPage"/> instance if the page has already been affected
        /// by the transaction, otherwise null.</returns>
        public virtual IDataPage TryGetPage(int pageNumber)
        {
            IDataPage page;
            return this.writtenPages.TryGetValue(pageNumber, out page) ? page : null;
        }

        /// <summary>
        /// Marks the transaction log as as committed.
        /// </summary>
        public virtual void MarkAsCommitted()
        {
            this.logFileManager.LogState = TransactionLogState.TransactionCommitted;
        }

        /// <summary>
        /// Logs the original data from the affected pages into the log file.
        /// </summary>
        public virtual void LogExistingDataForAffectedPages()
        {
            if (this.TransactionComplete)
            {
                throw new PersistenceException("Transaction log has already been written. Unable to log further information.");
            }

            // Log the page manager header if required
            if (this.PageManagerHeaderWritten)
            {
                this.logFileManager.LogDataFrom(LogEntryDataType.PageManagerHeader, this.dataFileManager, DataFileManager.HeaderSize, DataFileManager.PageManagerHeaderSize);
            }

            // Log each of the affected pages
            foreach (var page in this.pageWriteLevels)
            {
                var pageOffset = DataFileManager.GetPageOffset(page.Key);
                var pageWriteLevel = page.Value;

                if ((pageWriteLevel & PageWriteLevels.Created) == PageWriteLevels.Created)
                {
                    // If the page was created after the original extent of the file, don't bother logging it
                    if (page.Key < this.OriginalPageCount)
                    {
                        // The page was created from an old empty one - just log the old header data.
                        this.logFileManager.LogDataFrom(LogEntryDataType.PageHeader, this.dataFileManager, pageOffset, DataFileManager.PageHeaderSize);
                    }
                }
                else if ((pageWriteLevel & PageWriteLevels.Body) == PageWriteLevels.Body)
                {
                    // Log the original data from the page - only the data that was actually written (i.e not the unused bytes at the end)
                    IDataPageHeader existingHeader;
                    using (var headerReader = this.dataFileManager.GetDataReader(DataFileManager.GetPageOffset(page.Key), DataFileManager.PageHeaderSize))
                    {
                        existingHeader = headerReader.RestorePageHeader(page.Key);
                    }

                    this.logFileManager.LogDataFrom(LogEntryDataType.FullPage, this.dataFileManager, pageOffset, existingHeader.CurrentSize);
                }
                else if ((pageWriteLevel & PageWriteLevels.Header) == PageWriteLevels.Header)
                {
                    // Just log the original header
                    this.logFileManager.LogDataFrom(LogEntryDataType.PageHeader, this.dataFileManager, pageOffset, DataFileManager.PageHeaderSize);
                }
            }

            // Write the end of log marker
            this.logFileManager.EndLog();

            this.TransactionComplete = true;
        }

        /// <summary>
        /// Gets the pages numbers for the pages that were recorded as being affected with the given write level flag.
        /// </summary>
        /// <param name="writeLevel">The write level to get the affected pages for.</param>
        /// <returns>The relevant list of affected page numbers.</returns>
        private IEnumerable<int> GetPagesNumbersWithWriteFlag(PageWriteLevels writeLevel)
        {
            return from v in this.pageWriteLevels
                   where (v.Value & writeLevel) == writeLevel
                   select v.Key;
        }

        /// <summary>
        /// Gets the pages with given write level flag.
        /// </summary>
        /// <param name="writeLevel">The write level to match.</param>
        /// <returns>The relevant list of pages.</returns>
        private IEnumerable<IDataPage> GetPagesWithWriteFlag(PageWriteLevels writeLevel)
        {
            return this.GetPagesNumbersWithWriteFlag(writeLevel).Select(p => this.writtenPages[p]);
        }
    }
}
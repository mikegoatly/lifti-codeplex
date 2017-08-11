// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Lifti.Extensibility;
    using Lifti.Persistence.IO;

    /// <summary>
    /// The implementation of the page manager. This is responsible for managing the mapping of physical pages
    /// to the underlying data store, as well as the creation of new pages.
    /// </summary>
    /// <typeparam name="TItem">The type of the item contained in the index.</typeparam>
    public class PageManager<TItem> : IPageManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageManager&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="pageCache">The page cache.</param>
        /// <param name="settings">The settings for this instance.</param>
        /// <param name="dataFileManager">The data file manager for this instance.</param>
        /// <param name="typePersistence">The type persistence implementation that will manage reading and writing
        /// type data to and from the persistence backing store.</param>
        /// <param name="extensibilityService">The extensibility service.</param>
        public PageManager(IPageCache pageCache, IPersistenceSettings settings, IDataFileManager dataFileManager, ITypePersistence<TItem> typePersistence, IIndexExtensibilityService<TItem> extensibilityService)
        {
            if (pageCache == null)
            {
                throw new ArgumentNullException(nameof(pageCache));
            }

            if (typePersistence == null)
            {
                throw new ArgumentNullException(nameof(typePersistence));
            }

            if (dataFileManager == null)
            {
                throw new ArgumentNullException(nameof(dataFileManager));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (extensibilityService == null)
            {
                throw new ArgumentNullException(nameof(extensibilityService));
            }

            this.ExtensibilityService = extensibilityService;
            this.PageCache = pageCache;
            this.ItemDataPages = new DataPageCollection();
            this.IndexNodeDataPages = new DataPageCollection();
            this.ItemNodeIndexDataPages = new DataPageCollection();
            this.availablePages = new Queue<int>();

            this.settings = settings;
            this.typePersistence = typePersistence;
            this.FileManager = dataFileManager;
        }

        /// <summary>
        /// The version of the persisted file that this instance is capable of managing.
        /// </summary>
        private const short CurrentFileVersion = 1;

        /// <summary>
        /// The common marker bytes that are stored in the header of the file.
        /// </summary>
        private static readonly byte[] headerBytes = { 0x4C, 0x49, 0x46, 0x54, 0x4D, 0x47 };

        /// <summary>
        /// The type persistence instance capable of reading and writing the type to and from 
        /// a binary serializer.
        /// </summary>>
        private readonly ITypePersistence<TItem> typePersistence;

        /// <summary>
        /// The persistence settings for this instance.
        /// </summary>
        private readonly IPersistenceSettings settings;

        /// <summary>
        /// The thread synchronization object.
        /// </summary>
        private readonly object syncObj = new object();

        /// <summary>
        /// The available pages.
        /// </summary>
        private Queue<int> availablePages;

        /// <summary>
        /// The next available sequential item id.
        /// </summary>
        private int nextItemId;

        /// <summary>
        /// The next available sequential index node id. Index node 0 is automatically allocated to the root node, therefore the
        /// next node id for a new index is always 1.
        /// </summary>
        private int nextIndexNodeId = 1;

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the page cache that this instance will use.
        /// </summary>
        /// <value>
        /// The page cache.
        /// </value>
        public IPageCache PageCache
        {
            get; }

        /// <summary>
        /// Gets the page headers of the data pages that list the items stored in 
        /// the persisted store.
        /// </summary>
        /// <value>The physical page numbers of the item data pages.</value>
        public virtual IDataPageCollection ItemDataPages { get; private set; }

        /// <summary>
        /// Gets the page headers for the data pages within the persisted store
        /// that contain information about the index nodes in the index structure.
        /// </summary>
        /// <value>The physical page numbers of the text index data pages.</value>
        public virtual IDataPageCollection IndexNodeDataPages { get; private set; }

        /// <summary>
        /// Gets the page headers for the data pages within the persisted store
        /// that contain the links between items and the index nodes that they are indexed against.
        /// </summary>
        /// <value>The item reference data pages.</value>
        public virtual IDataPageCollection ItemNodeIndexDataPages { get; private set; }

        /// <summary>
        /// Gets or sets the total number of pages in the persisted file store. This
        /// includes unallocated pages.
        /// </summary>
        /// <value>
        /// The total number of physical pages in the store, both allocated and unallocated.
        /// </value>
        public virtual int TotalPageCount { get; protected set; }

        /// <summary>
        /// Gets the number of unused pages.
        /// </summary>
        /// <value>The unused page count.</value>
        public virtual int UnusedPageCount => this.availablePages.Count;

        /// <summary>
        /// Gets the I/O manager for this instance's underlying data file.
        /// </summary>
        public IDataFileManager FileManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the extensibility service.
        /// </summary>
        /// <value>
        /// The extensibility service.
        /// </value>
        protected IIndexExtensibilityService<TItem> ExtensibilityService
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes this instance, loading and verifying any existing state from
        /// the persisted backing store.
        /// </summary>
        public virtual void Initialize()
        {
            this.Initialized = true;

            if (this.FileManager.IsNewFile)
            {
                this.InitializeNewPersistedFile();
            }
            else
            {
                this.ReadHeader();
            }
        }

        /// <inheritdoc />
        public virtual void Flush()
        {
        }

        /// <summary>
        /// Allocates the new internal item id.
        /// </summary>
        /// <returns>
        /// The new id.
        /// </returns>
        public virtual int AllocateNewItemId()
        {
            this.VerifyInitializationState();

            lock (this.syncObj)
            {
                var nextId = this.nextItemId++;

                this.PersistPageManagerHeader();

                return nextId;
            }
        }

        /// <summary>
        /// Allocates a new internal index node id.
        /// </summary>
        /// <returns>
        /// The new id.
        /// </returns>
        public virtual int AllocateNewIndexNodeId()
        {
            this.VerifyInitializationState();

            lock (this.syncObj)
            {
                var nextId = this.nextIndexNodeId++;

                this.PersistPageManagerHeader();

                return nextId;
            }
        }

        /// <summary>
        /// Gets the data page associated to the given page header.
        /// </summary>
        /// <param name="pageHeader">The header of the page to get.</param>
        /// <returns>The loaded data page.</returns>
        public virtual IDataPage GetPage(IDataPageHeader pageHeader)
        {
            if (pageHeader == null)
            {
                throw new ArgumentNullException(nameof(pageHeader));
            }

            this.VerifyInitializationState();

            return this.PageCache.GetCachedPage(pageHeader, this.RestoreDataPage);
        }

        /// <summary>
        /// Gets the data page held at the given page number.
        /// </summary>
        /// <param name="pageNumber">
        /// The physical page number of the data page to read.
        /// </param>
        /// <returns>
        /// The loaded data page.
        /// </returns>
        public virtual IDataPage GetPage(int pageNumber)
        {
            this.VerifyInitializationState();

            return this.GetPage(this.PageCache.GetHeader(pageNumber));
        }

        /// <summary>
        /// Saves the changes in the given page. If the page has no remaining entries, it will be marked as unused and 
        /// returned to the unused page pool.
        /// </summary>
        /// <param name="page">The page to save.</param>
        public virtual void SavePage(IDataPage page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            if (page.Header.DataPageType == DataPageType.Unused)
            {
                throw new ArgumentException("Should not be saving a page with type of Unused", nameof(page));
            }

            this.VerifyInitializationState();

            if (page.Header.EntryCount == 0)
            {
                this.InvalidatePage(page);
            }
            else
            {
                this.PersistPage(page);
            }
        }

        /// <summary>
        /// Creates a new page after the given data page. All required page references
        /// will be updated. Affected page headers will be persisted to the backing file store.
        /// </summary>
        /// <typeparam name="TDataPage">The type of the data page.</typeparam>
        /// <param name="previousPage">The page to create the new page after.</param>
        /// <returns>
        /// The new page that follows on directly from the given page.
        /// </returns>
        public virtual TDataPage CreatePage<TDataPage>(TDataPage previousPage)
            where TDataPage : IDataPage
        {
            this.VerifyInitializationState();

            var newPage = (TDataPage)this.AllocateNewPage(previousPage.Header.DataPageType, previousPage.Header.PageNumber, previousPage.Header.NextPage, previousPage.Header.LastEntry);

            // Update the appropriate linked list
            var list = this.GetListForPageType(previousPage.Header.DataPageType);
            list.Insert(newPage.Header, previousPage.Header);

            // Update the links between the pages
            previousPage.Header.NextPage = newPage.Header.PageNumber;

            // Persist only the headers back - the contents of the pages haven't changed.
            this.PersistPageHeader(previousPage);
            this.PersistPageHeader(newPage);

            // Update the back reference from the next page, if there is one
            if (newPage.Header.NextPage != null)
            {
                var followingPage = this.GetPage(newPage.Header.NextPage.Value);
                followingPage.Header.PreviousPage = newPage.Header.PageNumber;
                this.PersistPageHeader(followingPage);
            }

            // Cache the new page
            this.PageCache.CachePage(newPage);
            this.PageCache.CacheHeader(newPage.Header);

            return newPage;
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
                if (this.FileManager != null)
                {
                    this.FileManager.Dispose();
                    this.FileManager = null;
                }
            }
        }

        /// <summary>
        /// Writes the overall file header.
        /// </summary>
        protected virtual void PersistPageManagerHeader()
        {
            // The page manager header data starts immediately after the main header.
            using (var writer = this.FileManager.GetDataWriter(DataFileManager.HeaderSize, DataFileManager.PageManagerHeaderSize))
            {
                writer.Write(this.ItemDataPages.Select(p => p.PageNumber).First());
                writer.Write(this.IndexNodeDataPages.Select(p => p.PageNumber).First());
                writer.Write(this.ItemNodeIndexDataPages.Select(p => p.PageNumber).First());
                writer.Write(this.TotalPageCount);
                writer.Write(this.nextItemId);
                writer.Write(this.nextIndexNodeId); 
            }
        }

        /// <summary>
        /// Verifies the initialization state of the page manager.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="PageManager&lt;TItem&gt;.Initialize"/> method
        /// hasn't been called yet.</exception>
        protected void VerifyInitializationState()
        {
            if (!this.Initialized)
            {
                throw new InvalidOperationException("Page manager not initialized");
            }
        }

        /// <summary>
        /// Persists the header of the given page to the underlying data store.
        /// </summary>
        /// <param name="page">The page whose header should be persisted.</param>
        protected virtual void PersistPageHeader(IDataPage page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var offset = DataFileManager.GetPageOffset(page.Header.PageNumber);
            using (var writer = this.FileManager.GetDataWriter(offset, DataFileManager.PageSize))
            {
                writer.PersistPageHeader(page.Header);
            }
        }

        /// <summary>
        /// Persists the given page to the underlying data store.
        /// </summary>
        /// <param name="page">The page to persist.</param>
        protected virtual void PersistPage(IDataPage page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var offset = DataFileManager.GetPageOffset(page.Header.PageNumber);
            using (var writer = this.FileManager.GetDataWriter(offset, DataFileManager.PageSize))
            {
                writer.PersistPageHeader(page.Header);

                switch (page.Header.DataPageType)
                {
                    case DataPageType.IndexNode:
                        writer.PersistIndexNodeDataPage((IndexNodeDataPage)page);
                        break;

                    case DataPageType.Items:
                        writer.PersistItemIndexPage((ItemIndexDataPage<TItem>)page, this.typePersistence);
                        break;

                    case DataPageType.ItemNodeIndex:
                        writer.PersistItemNodeIndexDataPage((ItemNodeIndexDataPage)page);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the page header for the given physical page number.
        /// </summary>
        /// <param name="pageNumber">
        /// The physical page number of the data page being read.
        /// </param>
        /// <returns>
        /// The loaded <see cref="IDataPageHeader"/> instance.
        /// </returns>
        protected IDataPageHeader ReadPageHeader(int pageNumber)
        {
            if (pageNumber >= this.TotalPageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Requested page number out of range");
            }

            // The header doesn't exist in the cache - load it.
            var offset = DataFileManager.GetPageOffset(pageNumber);
            using (var reader = this.FileManager.GetDataReader(offset, DataFileManager.PageSize))
            {
                return this.GetPageHeader(reader, pageNumber);
            }
        }

        /// <summary>
        /// Reloads the page manager header.
        /// </summary>
        protected void ReloadPageManagerHeader()
        {
            this.ItemDataPages = new DataPageCollection();
            this.IndexNodeDataPages = new DataPageCollection();
            this.ItemNodeIndexDataPages = new DataPageCollection();

            this.ReadPageManagerHeader();
        }

        /// <summary>
        /// Determines the page usage throughout all the persisted pages, including which of them are
        /// currently unallocated.
        /// </summary>
        /// <param name="firstItemDataPageNumber">The first item data page number.</param>
        /// <param name="firstIndexNodeDataPageNumber">The first index node data page number.</param>
        /// <param name="firstItemNodeIndexDataPageNumber">The first item node index data page number.</param>
        private void DeterminePageUsage(int firstItemDataPageNumber, int firstIndexNodeDataPageNumber, int firstItemNodeIndexDataPageNumber)
        {
            this.LoadPageReferences(DataPageType.Items, firstItemDataPageNumber);
            this.LoadPageReferences(DataPageType.IndexNode, firstIndexNodeDataPageNumber);
            this.LoadPageReferences(DataPageType.ItemNodeIndex, firstItemNodeIndexDataPageNumber);

            this.CalculateUnusedPages();
        }

        /// <summary>
        /// Derives the list of unused pages by looking at the pages that are currently in use.
        /// </summary>
        private void CalculateUnusedPages()
        {
            var availablePageList =
                Enumerable.Range(0, this.TotalPageCount)
                    .Except(this.ItemDataPages.Concat(this.IndexNodeDataPages).Concat(this.ItemNodeIndexDataPages).Select(h => h.PageNumber))
                    .ToArray();

            this.VerifyAvailablePages(availablePageList);

            this.availablePages = new Queue<int>(availablePageList.Reverse());
        }

        /// <summary>
        /// Extends the number of pages managed by the page manager- this affects the physical file structure.
        /// Note that the page manager header will NOT be persisted - it is the responsibility of the caller to ensure
        /// that this happens at an appropriate time.
        /// </summary>
        private void ExtendPages()
        {
            // First extend the file by the required amount
            var newPageCount = this.settings.GrowPageCount;
            if (newPageCount < 2)
            {
                throw new InvalidOperationException("Attempting to extend pages by less than 2");
            }

            var currentLength = DataFileManager.TotalHeaderSize + (this.TotalPageCount * DataFileManager.PageSize);
            this.FileManager.ExtendStream(currentLength + (DataFileManager.PageSize * newPageCount));

            // Initialize the new pages so they are in their initial "unallocated" state.
            for (int i = 0, newPageNumber = this.TotalPageCount; i < newPageCount; i++, newPageNumber++)
            {
                var header = new DataPageHeader(newPageNumber);
                var offset = DataFileManager.GetPageOffset(header.PageNumber);
                using (var writer = this.FileManager.GetDataWriter(offset, DataFileManager.PageSize))
                {
                    writer.PersistPageHeader(header);
                }

                // Store the new page in the unallocated page queue.
                this.availablePages.Enqueue(newPageNumber);
            }

            this.TotalPageCount += newPageCount;
        }

        /// <summary>
        /// Restores the data page associated to the given header.
        /// </summary>
        /// <param name="header">The header of the page to restore.</param>
        /// <returns>The restored data page.</returns>
        private IDataPage RestoreDataPage(IDataPageHeader header)
        {
            IDataPage dataPage = null;
            var offset = DataFileManager.GetPageOffset(header.PageNumber) + DataFileManager.PageHeaderSize;
            using (var dataReader = this.FileManager.GetDataReader(offset, DataFileManager.PageSize - DataFileManager.PageHeaderSize))
            {
                switch (header.DataPageType)
                {
                    case DataPageType.Items:
                        dataPage = dataReader.RestoreItemIndexDataPage(header, this.typePersistence);
                        break;

                    case DataPageType.IndexNode:
                        dataPage = dataReader.RestoreIndexNodePage(header);
                        break;

                    case DataPageType.ItemNodeIndex:
                        dataPage = dataReader.RestoreItemNodeIndexDataPage(header);
                        break;

                    case DataPageType.Unused:
                        throw new PersistenceException("Attempt to read unused page");
                }
            }

            if (dataPage == null)
            {
                throw new PersistenceException("Attempt to read unknown page type");
            }

            return dataPage;
        }

        /// <summary>
        /// Invalidates the given page, updating all the relevant links around it.
        /// </summary>
        /// <param name="page">The page to invalidate.</param>
        private void InvalidatePage(IDataPage page)
        {
            if (page.Header.EntryCount != 0)
            {
                throw new PersistenceException("Cannot invalidate a page that still has entries.");
            }

            var list = this.GetListForPageType(page.Header.DataPageType);

            // Only invalidate the page if it is not the last one
            if (list.Count > 1)
            {
                list.Remove(page.Header);

                // Update the links between the pages
                if (page.Header.PreviousPage == null)
                {
                    // The page being invalidated is the first page in the list
                    // update the page manager header with the new first page number
                    this.PersistPageManagerHeader();
                }
                else
                {
                    var previousPage = this.GetPage(page.Header.PreviousPage.Value);
                    previousPage.Header.NextPage = page.Header.NextPage;
                    this.PersistPageHeader(previousPage);
                }

                // Update the back reference from the next page, if there is one
                if (page.Header.NextPage != null)
                {
                    var nextPage = this.GetPage(page.Header.NextPage.Value);
                    nextPage.Header.PreviousPage = page.Header.PreviousPage;
                    this.PersistPageHeader(nextPage);
                }

                // Blank out the header's references and set it to unused
                page.Header.PreviousPage = null;
                page.Header.NextPage = null;
                page.Header.DataPageType = DataPageType.Unused;

                // Store freed page in the unallocated page queue.
                this.availablePages.Enqueue(page.Header.PageNumber);
            }

            // Save the page header
            this.PersistPageHeader(page);
        }

        /// <summary>
        /// Gets the list of used pages for the given data page type.
        /// </summary>
        /// <param name="pageType">The data page type to get the list for.</param>
        /// <returns>The relevant list of data pages.</returns>
        private IDataPageCollection GetListForPageType(DataPageType pageType)
        {
            switch (pageType)
            {
                case DataPageType.IndexNode:
                    return this.IndexNodeDataPages;
                case DataPageType.ItemNodeIndex:
                    return this.ItemNodeIndexDataPages;
                case DataPageType.Items:
                    return this.ItemDataPages;
                default:
                    throw new InvalidOperationException("Unknown data page type");
            }
        }

        /// <summary>
        /// Allocates a new (empty) page of the requested type. This does not persist the data page.
        /// </summary>
        /// <param name="pageType">The type of the page to allocate.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="itemId">The item id to store in the page, even though this page is empty.</param>
        /// <returns>
        /// The new allocated data page.
        /// </returns>
        private IDataPage AllocateNewPage(DataPageType pageType, int? previousPage, int? nextPage, int itemId)
        {
            var pageNumber = this.AllocateNewPageNumber();

            var newHeader = new DataPageHeader(pageType, pageNumber, previousPage, nextPage, 0, itemId, itemId, DataFileManager.PageHeaderSize);

            switch (pageType)
            {
                case DataPageType.Items:
                    return new ItemIndexDataPage<TItem>(newHeader);
                case DataPageType.IndexNode:
                    return new IndexNodeDataPage(newHeader);
                case DataPageType.ItemNodeIndex:
                    return new ItemNodeIndexDataPage(newHeader);
                default:
                    throw new ArgumentException("Unable to allocate page type " + pageType, nameof(pageType));
            }
        }

        /// <summary>
        /// Allocates a new page number for use, extending the file if required.
        /// </summary>
        /// <returns>The newly allocated page number.</returns>
        private int AllocateNewPageNumber()
        {
            if (this.availablePages.Count == 0)
            {
                this.ExtendPages();

                // Save the changes to the page manager header (i.e. total page count)
                this.PersistPageManagerHeader();
            }

            return this.availablePages.Dequeue();
        }

        /// <summary>
        /// Gets the page header for the given page number using the provided <see cref="BinaryReader"/>. 
        /// The <see cref="BinaryReader"/> must be already at the starting location of the data page.
        /// </summary>
        /// <param name="dataReader">
        /// The data reader to read from.
        /// </param>
        /// <param name="pageNumber">
        /// The physical page number of the data page being read.
        /// </param>
        /// <returns>
        /// The loaded <see cref="DataPageHeader"/> instance.
        /// </returns>
        private IDataPageHeader GetPageHeader(BinaryReader dataReader, int pageNumber)
        {
            if (pageNumber >= this.TotalPageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            }

            return dataReader.RestorePageHeader(pageNumber);
        }

        /// <summary>
        /// Processes through all the page references starting from the given physical data page number. The list
        /// of page numbers is expected to have been populated with the starting page number by this point.
        /// </summary>
        /// <param name="pageType">The expected <see cref="DataPageType"/> of the pages that will be referenced.</param>
        /// <param name="firstPageNumber">The first page number.</param>
        private void LoadPageReferences(DataPageType pageType, int firstPageNumber)
        {
            IDataPageHeader header;
            var pages = this.GetListForPageType(pageType);

            int? lastDataPageId = null;
            int? nextDataPageId = firstPageNumber;

            // Keep track of the visited pages to detect circular references
            var visitedPages = new HashSet<int>();

            do
            {
                if (visitedPages.Contains(nextDataPageId.Value))
                {
                    throw new PersistenceException("Circular reference detected in page references - file may be corrupt");
                }

                visitedPages.Add(nextDataPageId.Value);
                header = this.PageCache.GetHeader(nextDataPageId.GetValueOrDefault(), this.ReadPageHeader);

                pages.InsertLast(header);

                if (header.DataPageType != pageType)
                {
                    throw new PersistenceException("Unexpected page type encountered - file may be corrupted");
                }

                if (header.PreviousPage != lastDataPageId)
                {
                    throw new PersistenceException("Data page integrity not consistent");
                }

                lastDataPageId = nextDataPageId;
                nextDataPageId = header.NextPage;
            }
            while (nextDataPageId.HasValue);
        }

        /// <summary>
        /// Verifies that the available pages are actually marked as such.
        /// </summary>
        /// <param name="availablePageNumbers">
        /// The available pages.
        /// </param>
        private void VerifyAvailablePages(IEnumerable<int> availablePageNumbers)
        {
            if (availablePageNumbers.Any(p => this.ReadPageHeader(p).DataPageType != DataPageType.Unused))
            {
                throw new PersistenceException("Orphaned data encountered - page expected to be unused");
            }
        }

        /// <summary>
        /// Reads the complete file header, including verification data and initial page references.
        /// </summary>
        private void ReadHeader()
        {
            this.VerifyFileHeader();
            this.ReadPageManagerHeader();
        }

        /// <summary>
        /// Verifies the file header.
        /// </summary>
        private void VerifyFileHeader()
        {
            using (var reader = this.FileManager.GetDataReader(0, DataFileManager.HeaderSize))
            {
                if (!headerBytes.SequenceEqual(reader.ReadBytes(6)))
                {
                    throw new PersistenceException("Invalid file header encountered in persisted file");
                }

                if (reader.ReadInt16() != CurrentFileVersion)
                {
                    throw new PersistenceException("Incompatible file version encountered");
                }
            }
        }

        /// <summary>
        /// Initializes a new persisted file.
        /// </summary>
        private void InitializeNewPersistedFile()
        {
            // Extend the file to accommodate the data.
            this.ExtendPages();

            // Write the magic validation data at the start of the file
            this.WriteFileHeader();

            // Allocate the first item and text index pages
            var firstItemPage = this.AllocateNewPage(DataPageType.Items, null, null, 0);
            var firstIndexNodePage = this.AllocateNewPage(DataPageType.IndexNode, null, null, 0);
            var firstItemNodePage = this.AllocateNewPage(DataPageType.ItemNodeIndex, null, null, 0);
            this.ItemDataPages.InsertFirst(firstItemPage.Header);
            this.IndexNodeDataPages.InsertFirst(firstIndexNodePage.Header);
            this.ItemNodeIndexDataPages.InsertFirst(firstItemNodePage.Header);

            // Cache the new page headers
            this.PageCache.CacheHeader(firstItemPage.Header);
            this.PageCache.CacheHeader(firstIndexNodePage.Header);
            this.PageCache.CacheHeader(firstItemNodePage.Header);

            // Write the page manager data, followed by the pages.
            this.PersistPageManagerHeader();
            this.PersistPage(firstItemPage);
            this.PersistPage(firstIndexNodePage);
            this.PersistPage(firstItemNodePage);

            this.Flush();
        }

        /// <summary>
        /// Writes the overall file header.
        /// </summary>
        private void WriteFileHeader()
        {
            using (var writer = this.FileManager.GetDataWriter(0, DataFileManager.HeaderSize))
            {
                writer.Write(headerBytes);
                writer.Write(CurrentFileVersion);
            }
        }

        /// <summary>
        /// Reads the page manager header from the data file and prepares the lists
        /// of page references.
        /// </summary>
        private void ReadPageManagerHeader()
        {
            int firstItemDataPageNumber;
            int firstIndexNodeDataPageNumber;
            int firstItemNodeIndexDataPageNumber;

            // The page manager header data starts immediately after the main header.
            using (var reader = this.FileManager.GetDataReader(DataFileManager.HeaderSize, DataFileManager.PageManagerHeaderSize))
            {
                firstItemDataPageNumber = reader.ReadInt32();
                firstIndexNodeDataPageNumber = reader.ReadInt32();
                firstItemNodeIndexDataPageNumber = reader.ReadInt32();
                this.TotalPageCount = reader.ReadInt32();
                this.nextItemId = reader.ReadInt32();
                this.nextIndexNodeId = reader.ReadInt32();
            }

            this.DeterminePageUsage(firstItemDataPageNumber, firstIndexNodeDataPageNumber, firstItemNodeIndexDataPageNumber);
        }
    }
}
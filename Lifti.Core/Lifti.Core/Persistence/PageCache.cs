// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The page cache implementation. This implementation is thread safe.
    /// </summary>
    /// <remarks>Pages are stored in the cache until the <see cref="PageCache.MaxCachedPages"/> limit is reached. When this
    /// limit is exceeded pages that were last accessed the longest time ago will be purged - this timing is only stored to 
    /// to the nearest second (i.e. milliseconds are not taken into account). If multiple pages were requested at the same
    /// time, the pages that have been accessed the least are purged first.</remarks>
    public class PageCache : IPageCache
    {
        /// <summary>
        /// The cache of data pages, keyed on their page number.
        /// </summary>
        private readonly Dictionary<int, CachedPage> cache = new Dictionary<int, CachedPage>();

        /// <summary>
        /// The priority of the data pages in the cache. This is ordered by the page priority.
        /// </summary>
        private readonly List<CachedPage> pagePriority = new List<CachedPage>();

        /// <summary>
        /// The comparer to use when sorting the page priority list.
        /// </summary>
        private readonly CachedPageComparer cachedPageComparer = new CachedPageComparer();

        /// <summary>
        /// The cache of data page headers, keyed on their page number.
        /// </summary>
        private readonly Dictionary<int, IDataPageHeader> headerCache = new Dictionary<int, IDataPageHeader>();

        /// <summary>
        /// The thread synchronization object.
        /// </summary>
        private readonly object syncObj = new object();

        /// <summary>
        /// The maximum number of cached pages.
        /// </summary>
        private int maxCachedPages = 1000;

        /// <summary>
        /// Gets or sets the maximum number of cached pages. The default is 1000.
        /// </summary>
        /// <value>
        /// The maximum number of cached pages.
        /// </value>
        public int MaxCachedPages
        {
            get
            {
                return this.maxCachedPages;
            }

            set
            {
                if (this.maxCachedPages != value)
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("Value must not be less than 0", nameof(value));
                    }

                    this.maxCachedPages = value;
                    this.TruncateCache();
                }
            }
        }

        /// <summary>
        /// Determines whether a page is currently cached.
        /// </summary>
        /// <param name="pageNumber">The page number of the page to check for.</param>
        /// <returns><c>true</c> if the page is cache, otherwise <c>false</c></returns>
        public bool IsPageCached(int pageNumber)
        {
            lock (this.syncObj)
            {
                return this.cache.ContainsKey(pageNumber);
            }
        }

        /// <summary>
        /// Purges the given pages from the cache.
        /// </summary>
        /// <param name="pages">The page numbers to purge.</param>
        public void PurgePages(IEnumerable<int> pages)
        {
            if (pages == null)
            {
                throw new ArgumentNullException(nameof(pages));
            }

            lock (this.syncObj)
            {
                foreach (var pageNumber in pages)
                {
                    CachedPage page;
                    if (this.cache.TryGetValue(pageNumber, out page))
                    {
                        // Remove the page from both of the cache collections.
                        this.RemovePage(page);
                    }
                }
            }
        }

        /// <summary>
        /// Purges the given headers from the cache.
        /// </summary>
        /// <param name="headers">The page numbers of the headers to purge.</param>
        public void PurgeHeaders(IEnumerable<int> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            lock (this.syncObj)
            {
                foreach (var pageNumber in headers)
                {
                    this.headerCache.Remove(pageNumber);
                }
            }
        }

        /// <summary>
        /// Gets a page from the cache. If the page doesn't exist in the cache it is loaded from the delegate and cached
        /// for later use before being returned.
        /// </summary>
        /// <param name="pageHeader">The header for the page to load from the cache.</param>
        /// <param name="loadPage">The delegate capable of loading the page if it doesn't exist in the
        /// cache.</param>
        /// <returns>
        /// The cached <see cref="IDataPage"/> instance.
        /// </returns>
        public IDataPage GetCachedPage(IDataPageHeader pageHeader, Func<IDataPageHeader, IDataPage> loadPage)
        {
            if (pageHeader == null)
            {
                throw new ArgumentNullException(nameof(pageHeader));
            }

            if (loadPage == null)
            {
                throw new ArgumentNullException(nameof(loadPage));
            }

            lock (this.syncObj)
            {
                var pageNumber = pageHeader.PageNumber;
                CachedPage page;
                if (this.cache.TryGetValue(pageNumber, out page))
                {
                    this.UpdateCachedPagePriority(page);
                }
                else
                {
                    page = this.CachePageRemovingExcess(loadPage(pageHeader));
                }

                return page.Page;
            }
        }

        /// <summary>
        /// Gets the data page header for the given page number. This will throw an exception if the
        /// header is not cached, as the page cache will cache all headers indefinitely.
        /// </summary>
        /// <param name="pageNumber">The page number of the header to get.</param>
        /// <returns>The cached data page header.</returns>
        public IDataPageHeader GetHeader(int pageNumber)
        {
            try
            {
                lock (this.syncObj)
                {
                    return this.headerCache[pageNumber];
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new PersistenceException("Internal error - an un-cached header was requested. (Page number " + pageNumber + ")", ex);
            }
        }

        /// <summary>
        /// Gets a header from the cache. If the header doesn't exist in the cache it is loaded from the delegate and cached
        /// for later use before being returned.
        /// </summary>
        /// <param name="pageNumber">The page number of the header to get.</param>
        /// <param name="loadHeader">The delegate capable of loading header if it doesn't exist in the cache.</param>
        /// <returns>
        /// The cached data page header.
        /// </returns>
        public IDataPageHeader GetHeader(int pageNumber, Func<int, IDataPageHeader> loadHeader)
        {
            if (loadHeader == null)
            {
                throw new ArgumentNullException(nameof(loadHeader));
            }

            lock (this.syncObj)
            {
                IDataPageHeader header;
                if (!this.headerCache.TryGetValue(pageNumber, out header))
                {
                    header = loadHeader(pageNumber);
                    this.headerCache.Add(pageNumber, header);
                }

                return header;
            }
        }

        /// <summary>
        /// Caches the given page.
        /// </summary>
        /// <param name="page">The page to cache.</param>
        public void CachePage(IDataPage page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            lock (this.syncObj)
            {
                this.CachePageRemovingExcess(page);
            }
        }

        /// <summary>
        /// Caches the data page header.
        /// </summary>
        /// <param name="header">The data page header to cache.</param>
        public void CacheHeader(IDataPageHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            lock (this.syncObj)
            {
                this.headerCache[header.PageNumber] = header;
            }
        }

        /// <summary>
        /// Refreshes the last access date/time and increments the access count for the
        /// given cached page.
        /// </summary>
        /// <param name="page">The page to update the access information for.</param>
        private void UpdateCachedPagePriority(CachedPage page)
        {
            page.LastAccess = DateTime.Now;
            page.AccessCount++;
        }

        /// <summary>
        /// Removes the given page from the both cache collections.
        /// </summary>
        /// <param name="page">The page to remove.</param>
        private void RemovePage(CachedPage page)
        {
            this.cache.Remove(page.Page.Header.PageNumber);
            this.pagePriority.Remove(page);
        }

        /// <summary>
        /// Caches the given page, truncating the cache if required.
        /// </summary>
        /// <param name="page">The page being added to the cache.</param>
        /// <returns>The information about the cached page</returns>
        private CachedPage CachePageRemovingExcess(IDataPage page)
        {
            var cachedPage = new CachedPage(page);

            this.cache[page.Header.PageNumber] = cachedPage;

            this.pagePriority.Add(cachedPage);

            this.TruncateCache();

            return cachedPage;
        }

        /// <summary>
        /// Truncates the cache, removing the least-used pages if needed.
        /// </summary>
        private void TruncateCache()
        {
            if (this.pagePriority.Count <= this.MaxCachedPages)
            {
                return;
            }

            this.pagePriority.Sort(this.cachedPageComparer);
            while (this.pagePriority.Count > this.MaxCachedPages)
            {
                var page = this.pagePriority[0];
                this.cache.Remove(page.Page.Header.PageNumber);
                this.pagePriority.RemoveAt(0);

            }
        }
    }
}

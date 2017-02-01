// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The interface implemented by page caches.
    /// </summary>
    public interface IPageCache
    {
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
        IDataPage GetCachedPage(IDataPageHeader pageHeader, Func<IDataPageHeader, IDataPage> loadPage);

        /// <summary>
        /// Gets a header from the cache. If the header doesn't exist in the cache it is loaded from the delegate and cached
        /// for later use before being returned.
        /// </summary>
        /// <param name="pageNumber">The page number of the header to get.</param>
        /// <param name="loadHeader">The delegate capable of loading header if it doesn't exist in the cache.</param>
        /// <returns>
        /// The cached data page header.
        /// </returns>
        IDataPageHeader GetHeader(int pageNumber, Func<int, IDataPageHeader> loadHeader);

        /// <summary>
        /// Gets the data page header for the given page number. This will throw an exception if the
        /// header is not cached, as the page cache will cache all headers indefinitely.
        /// </summary>
        /// <param name="pageNumber">The page number of the header to get.</param>
        /// <returns>The cached data page header.</returns>
        IDataPageHeader GetHeader(int pageNumber);

        /// <summary>
        /// Determines whether a page is currently cached.
        /// </summary>
        /// <param name="pageNumber">The page number of the page to check for.</param>
        /// <returns><c>true</c> if the page is cache, otherwise <c>false</c></returns>
        bool IsPageCached(int pageNumber);

        /// <summary>
        /// Caches the given page.
        /// </summary>
        /// <param name="page">The page to cache.</param>
        void CachePage(IDataPage page);

        /// <summary>
        /// Caches the data page header.
        /// </summary>
        /// <param name="header">The data page header to cache.</param>
        void CacheHeader(IDataPageHeader header);

        /// <summary>
        /// Purges the given pages from the cache.
        /// </summary>
        /// <param name="pages">The page numbers to purge.</param>
        void PurgePages(IEnumerable<int> pages);

        /// <summary>
        /// Purges the given headers from the cache.
        /// </summary>
        /// <param name="headers">The page numbers of the headers to purge.</param>
        void PurgeHeaders(IEnumerable<int> headers);
    }
}
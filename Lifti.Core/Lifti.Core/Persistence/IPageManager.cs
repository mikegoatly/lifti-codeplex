// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;

    using Lifti.Persistence.IO;

    /// <summary>
    /// The interface implemented by classes capable of managing physical data pages.
    /// </summary>
    public interface IPageManager : IDisposable
    {
        /// <summary>
        /// Gets the underlying data file manager.
        /// </summary>
        /// <value>The data file manager.</value>
        IDataFileManager FileManager { get; }

        /// <summary>
        /// Gets the page headers of the data pages that list the items stored in 
        /// the persisted store.
        /// </summary>
        /// <value>The physical page numbers of the item data pages.</value>
        IDataPageCollection ItemDataPages { get; }

        /// <summary>
        /// Gets the page headers for the data pages within the persisted store
        /// that contain information about the index nodes in the index structure.
        /// </summary>
        /// <value>The physical page numbers of the text index data pages.</value>
        IDataPageCollection IndexNodeDataPages { get; }

        /// <summary>
        /// Gets the page headers for the data pages within the persisted store
        /// that contain the links between items and the index nodes that they are indexed against.
        /// </summary>
        /// <value>The item reference data pages.</value>
        IDataPageCollection ItemNodeIndexDataPages { get; }

        /// <summary>
        /// Gets the total number of pages in the persisted file store. This
        /// includes unallocated pages.
        /// </summary>
        /// <value>The total number of physical pages in the store, both allocated and unallocated.</value>
        int TotalPageCount { get; }

        /// <summary>
        /// Gets the number of unused pages.
        /// </summary>
        /// <value>The unused page count.</value>
        int UnusedPageCount { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        bool Initialized { get; }

        /// <summary>
        /// Initializes this instance, loading and verifying any existing state from
        /// the persisted backing store.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Flushes any unsaved pages to the backing store.
        /// </summary>
        void Flush();

        /// <summary>
        /// Gets the data page held at the given page number.
        /// </summary>
        /// <param name="pageNumber">The physical page number of the data page to read.</param>
        /// <returns>The loaded data page.</returns>
        IDataPage GetPage(int pageNumber);

        /// <summary>
        /// Gets the data page associated to the given page header.
        /// </summary>
        /// <param name="pageHeader">The header of the page to get.</param>
        /// <returns>The loaded data page.</returns>
        IDataPage GetPage(IDataPageHeader pageHeader);

        /// <summary>
        /// Creates a new page after the given data page. All required page references
        /// will be updated. Affected page headers will be persisted to the backing file store.
        /// </summary>
        /// <typeparam name="TDataPage">The type of the data page.</typeparam>
        /// <param name="previousPage">The page to create the new page after.</param>
        /// <returns>
        /// The new page that follows on directly from the given page.
        /// </returns>
        TDataPage CreatePage<TDataPage>(TDataPage previousPage) where TDataPage : IDataPage;

        /// <summary>
        /// Saves the changes in the given page. If the page has no remaining entries, it will be marked as unused and 
        /// returned to the unused page pool.
        /// </summary>
        /// <param name="page">The page to save.</param>
        void SavePage(IDataPage page);

        /// <summary>
        /// Allocates the new internal item id.
        /// </summary>
        /// <returns>The new id.</returns>
        int AllocateNewItemId();

        /// <summary>
        /// Allocates a new internal index node id.
        /// </summary>
        /// <returns>The new id.</returns>
        int AllocateNewIndexNodeId();
    }
}

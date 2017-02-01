// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Collections.Generic;

    /// <summary>
    /// The interface implemented by a list of data pages managed by a <see cref="PageManager&lt;TItem&gt;"/>. Pages are stored in logical rather than physical order.
    /// </summary>
    public interface IDataPageCollection : IEnumerable<IDataPageHeader>
    {
        /// <summary>
        /// Gets the number of pages managed by the collection.
        /// </summary>
        /// <value>The number of pages.</value>
        int Count { get; }

        /// <summary>
        /// Inserts the given page as the first page in the list.
        /// </summary>
        /// <param name="newPage">The new page.</param>
        void InsertFirst(IDataPageHeader newPage);

        /// <summary>
        /// Inserts the given page as the last page in the list.
        /// </summary>
        /// <param name="newPage">The new page.</param>
        void InsertLast(IDataPageHeader newPage);

        /// <summary>
        /// Inserts the specified given page immediately after another in the list.
        /// </summary>
        /// <param name="newPage">The new page.</param>
        /// <param name="afterPage">The page to insert after.</param>
        void Insert(IDataPageHeader newPage, IDataPageHeader afterPage);

        /// <summary>
        /// Removes the specified page.
        /// </summary>
        /// <param name="page">The page to remove.</param>
        void Remove(IDataPageHeader page);

        /// <summary>
        /// Finds the pages that contain the entries for the given entry.
        /// </summary>
        /// <param name="id">The internal id of the entry to locate the pages for.</param>
        /// <returns>The data page headers for the pages that contain the entry.</returns>
        IEnumerable<IDataPageHeader> FindPagesForEntry(int id);

        /// <summary>
        /// Finds the page that is the closest match to containing the given id.
        /// </summary>
        /// <param name="id">The internal id of the entry to locate the closest page for.</param>
        /// <returns>The data page header for the page that is closest to containing the given id.</returns>
        IDataPageHeader FindClosestPageForEntry(int id);
    }
}
// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The interface implemented by data pages.
    /// </summary>
    public interface IDataPage
    {
        /// <summary>
        /// Gets the header associated to the page.
        /// </summary>
        /// <value>The page's <see cref="IDataPageHeader"/> instance.</value>
        IDataPageHeader Header { get; }
    }

    /// <summary>
    /// The interface implemented by data pages.
    /// </summary>
    /// <typeparam name="TEntry">The type of the entry in the page.</typeparam>
    public interface IDataPage<TEntry> : IDataPage
        where TEntry : IDataPageEntry
    {
        /// <summary>
        /// Gets the entries in the data page.
        /// </summary>
        /// <value>The data page entries.</value>
        IEnumerable<TEntry> Entries { get; }

        /// <summary>
        /// Determines whether this instance can contain the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        ///   <c>true</c> if this instance can contain the specified entry; otherwise, <c>false</c>.
        /// </returns>
        bool CanContain(TEntry entry);

        /// <summary>
        /// Removes all entries from the page that match the given predicate.
        /// </summary>
        /// <param name="predicate">The predicate to test nodes against.</param>
        /// <returns><c>true</c> if at least one entry was removed, otherwise <c>false</c>.</returns>
        bool RemoveEntry(Func<TEntry, bool> predicate);

        /// <summary>
        /// Copies the entries that match the given predicate from the given page.
        /// </summary>
        /// <param name="from">The page to move the entries from.</param>
        /// <param name="predicate">The predicate that entries must match.</param>
        void MoveEntriesFrom(DataPage<TEntry> from, Predicate<TEntry> predicate);

        /// <summary>
        /// Adds the given entry to the page. The entry will be inserted into the correct order, based on its id.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        void AddEntry(TEntry entry);
    }
}
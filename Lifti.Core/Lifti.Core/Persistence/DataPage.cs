// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;

    using Lifti.Persistence.IO;

    /// <summary>
    /// The base class for data pages managed by the <see cref="PageManager&lt;TItem&gt;"/> class.
    /// </summary>
    /// <typeparam name="TEntry">The type of the entry in the data page.</typeparam>
    public abstract class DataPage<TEntry> : IDataPage<TEntry>
        where TEntry : IDataPageEntry
    {
        /// <summary>
        /// The entries in the page.
        /// </summary>
        private readonly LinkedList<TEntry> entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPage&lt;TEntry&gt;"/> class.
        /// </summary>
        /// <param name="header">The page header.</param>
        protected DataPage(IDataPageHeader header)
            : this(header, null)
        {   
            this.entries = new LinkedList<TEntry>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPage&lt;TEntry&gt;"/> class.
        /// </summary>
        /// <param name="header">The page header.</param>
        /// <param name="entries">The entries in the page.</param>
        protected DataPage(IDataPageHeader header, IEnumerable<TEntry> entries)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            this.Header = header;

            this.entries = entries != null ? new LinkedList<TEntry>(entries) : new LinkedList<TEntry>();
        }

        /// <summary>
        /// Gets the header associated to the page.
        /// </summary>
        /// <value>The page's <see cref="IDataPageHeader"/> instance.</value>
        public IDataPageHeader Header
        {
            get; }

        /// <summary>
        /// Gets the entries in the data page.
        /// </summary>
        /// <value>The data page entries.</value>
        public IEnumerable<TEntry> Entries => this.entries;

        /// <summary>
        /// Determines whether this instance can contain the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        ///   <c>true</c> if this instance can contain the specified entry; otherwise, <c>false</c>.
        /// </returns>
        public bool CanContain(TEntry entry)
        {
            return this.Header.CurrentSize + entry.Size <= DataFileManager.PageSize;
        }

        /// <summary>
        /// Adds the given entry to the page. The entry will be inserted into the correct order, based on its id.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        public void AddEntry(TEntry entry)
        {
            var entrySize = entry.Size;
            if (entrySize + this.Header.CurrentSize > DataFileManager.PageSize)
            {
                throw new PersistenceException("Added entry will not fit into the page.");
            }

            var id = entry.Id;
            if (this.Header.LastEntry <= id)
            {
                this.entries.AddLast(entry);
                this.Header.LastEntry = id;
            }
            else if (this.Header.FirstEntry >= id)
            {
                this.entries.AddFirst(entry);
                this.Header.FirstEntry = id;
            }
            else
            {
                this.entries.AddWhenOrLast(entry, r => r.Id >= id);
            }

            this.Header.EntryCount++;
            this.Header.CurrentSize += entrySize;
        }

        /// <summary>
        /// Removes all entries from the page that match the given predicate.
        /// </summary>
        /// <param name="predicate">The predicate to test nodes against.</param>
        /// <returns><c>true</c> if at least one entry was removed, otherwise <c>false</c>.</returns>
        public bool RemoveEntry(Func<TEntry, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            // It is possible for multiple entries to match the predicate
            var removed = false;
            var next = this.entries.First;
            while (next != null)
            {
                var node = next;
                next = node.Next;
                if (predicate(node.Value))
                {
                    this.entries.Remove(node);
                    this.Header.CurrentSize -= node.Value.Size;
                    removed = true;
                }
            }

            if (removed)
            {
                this.SynchronizeHeaderEntryRange();
            }

            return removed;
        }

        /// <summary>
        /// Copies the entries that match the given predicate from the given page.
        /// </summary>
        /// <param name="from">The page to move the entries from.</param>
        /// <param name="predicate">The predicate that entries must match.</param>
        public void MoveEntriesFrom(DataPage<TEntry> from, Predicate<TEntry> predicate)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(@from));    
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (from == this)
            {
                throw new ArgumentException("The page being copied from must be different to the current page", nameof(@from));    
            }

            var node = from.entries.First;
            while (node != null)
            {
                var nextNode = node.Next;
                if (predicate(node.Value))
                {
                    var size = node.Value.Size;
                    this.entries.AddLast(node.Value);
                    this.Header.CurrentSize += size;

                    from.entries.Remove(node);
                    from.Header.CurrentSize -= size;
                }

                node = nextNode;
            }

            from.SynchronizeHeaderEntryRange();
            this.SynchronizeHeaderEntryRange();
        }

        /// <summary>
        /// Synchronizes the header entry range.
        /// </summary>
        private void SynchronizeHeaderEntryRange()
        {
            if (this.entries.Count == 0)
            {
                this.Header.FirstEntry = 0;
                this.Header.LastEntry = 0;
            }
            else
            {
                this.Header.FirstEntry = this.entries.First.Value.Id;
                this.Header.LastEntry = this.entries.Last.Value.Id;
            }

            this.Header.EntryCount = this.entries.Count;
        }

#if DEBUG
        private int CalculateSize()
        {
            return DataFileManager.PageHeaderSize + System.Linq.Enumerable.Sum(this.Entries, e => e.Size);
        }
#endif
    }
}
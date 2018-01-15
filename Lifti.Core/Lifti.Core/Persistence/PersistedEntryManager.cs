// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The interface implemented by classes capable of managing individual entries in the persisted file store. These entries can be
    /// representative of items that have been indexed in the full text index, and information about how the index's nodes are 
    /// structured.
    /// </summary>
    /// <typeparam name="TKey">The type of the key stored in the index.</typeparam>
    public class PersistedEntryManager<TKey> : IPersistedEntryManager<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedEntryManager&lt;TKey&gt;"/> class.
        /// </summary>
        /// <param name="pageManager">The page manager the entry manager should use to access the underlying persisted store..</param>
        /// <param name="typePersistence">The type persistence instance. Used in this context to measure the site of an item.</param>
        public PersistedEntryManager(IPageManager pageManager, ITypePersistence<TKey> typePersistence)
        {
            if (pageManager == null)
            {
                throw new ArgumentNullException(nameof(pageManager));
            }

            if (typePersistence == null)
            {
                throw new ArgumentNullException(nameof(typePersistence));
            }

            if (!pageManager.Initialized)
            {
                pageManager.Initialize();
            }

            this.PageManager = pageManager;
            this.typePersistence = typePersistence;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.PageManager != null)
                {
                    this.PageManager.Dispose();
                    this.PageManager = null;
                }
            }
        }

        /// <summary>
        /// The type persistence instance. Used in this context to measure the site of an item.
        /// </summary>
        private readonly ITypePersistence<TKey> typePersistence;

        /// <summary>
        /// The lookup of item internal ids, keyed against the item itself.
        /// </summary>
        private Dictionary<TKey, int> itemIdLookup;

        /// <summary>
        /// The lookup of items keyed against their internal ids.
        /// </summary>
        private Dictionary<int, TKey> itemLookup;

        /// <summary>
        /// Gets the page manager the persisted entry manager is responsible for.
        /// </summary>
        /// <value>The associated <see cref="IPageManager"/> instance.</value>
        public IPageManager PageManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of items stored in the persisted backing store.
        /// </summary>
        /// <value>The number of items stored in the index.</value>
        public int ItemCount => this.itemLookup.Count;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets a value indicating whether the given item is contained within the index.
        /// </summary>
        /// <param name="item">The item to check the existence of.</param>
        /// <returns><c>true</c> if the item is contained in the index, otherwise <c>false</c>.</returns>
        public bool ItemIndexed(TKey item)
        {
            return this.itemIdLookup.ContainsKey(item);
        }

        /// <summary>
        /// Gets the internal id of the given item.
        /// </summary>
        /// <param name="item">The item to get the id for.</param>
        /// <returns>The internal id of the item.</returns>
        public int GetIdForItem(TKey item)
        {
            int itemId;
            if (!this.itemIdLookup.TryGetValue(item, out itemId))
            {
                throw new InvalidOperationException("Unknown item referenced in index.");
            }

            return itemId;
        }

        /// <summary>
        /// Gets the item associated to an internal id.
        /// </summary>
        /// <param name="itemId">The if of the item to get.</param>
        /// <returns>
        /// The internal id of the item.
        /// </returns>
        public TKey GetItemForId(int itemId)
        {
            TKey item;
            if (!this.itemLookup.TryGetValue(itemId, out item))
            {
                throw new InvalidOperationException("Unknown item referenced in index.");
            }

            return item;
        }

        /// <summary>
        /// Gets all the item entries held in the index.
        /// </summary>
        /// <returns>All the item entries.</returns>
        public IEnumerable<ItemEntry<TKey>> GetAllItemEntries()
        {
            foreach (var pageHeader in this.PageManager.ItemDataPages)
            {
                if (pageHeader.EntryCount > 0)
                {
                    var page = (ItemIndexDataPage<TKey>)this.PageManager.GetPage(pageHeader);
                    foreach (var entry in page.Entries)
                    {
                        yield return entry;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the index node entries, both <see cref="ItemReferenceIndexNodeEntry"/> and <see cref="NodeReferenceIndexNodeEntry"/>, for the
        /// node with the given logical id.
        /// </summary>
        /// <param name="logicalId">The logical id of the node to get the entries for.</param>
        /// <returns>An enumerable set of <see cref="IndexNodeEntryBase"/> that may be of type <see cref="ItemReferenceIndexNodeEntry"/> or <see cref="NodeReferenceIndexNodeEntry"/>.</returns>
        public IEnumerable<IndexNodeEntryBase> GetIndexNodeEntries(int logicalId)
        {
            var nodePages = this.GetIndexNodeDataPages(logicalId);

            foreach (var page in nodePages)
            {
                foreach (var entry in page.Entries)
                {
                    if (entry.Id == logicalId)
                    {
                        yield return entry;
                    }
                }
            }
        }

        /// <summary>
        /// Adds an index node reference entry.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="nodeId">The node id.</param>
        /// <param name="indexedCharacter">The indexed character.</param>
        public void AddIndexNodeReferenceEntry(int parentId, int nodeId, char indexedCharacter)
        {
            var reference = new NodeReferenceIndexNodeEntry(parentId, nodeId, indexedCharacter);
            this.AddNodeReferenceEntity(reference);
        }

        /// <summary>
        /// Removes an index node reference entry.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="nodeId">The node id.</param>
        public void RemoveIndexNodeReferenceEntry(int parentId, int nodeId)
        {
            // Get all the pages containing the entry
            this.RemoveIndexNodeEntry(parentId, nodeId, IndexNodeEntryType.NodeReference);
        }

        /// <summary>
        /// Removes a node item entry.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <param name="itemId">The item id.</param>
        public void RemoveNodeItemEntry(int nodeId, int itemId)
        {
            // Get all the pages containing the entry
            this.RemoveIndexNodeEntry(nodeId, itemId, IndexNodeEntryType.ItemReference);

            // Remove the reverse link
            this.RemoveItemNodeIndexEntry(itemId, nodeId);
        }

        /// <summary>
        /// Adds a node item entry.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <param name="itemId">The item id.</param>
        /// <param name="position">The word position.</param>
        public void AddNodeItemEntry(int nodeId, int itemId, int position)
        {
            // Add the reference from the index node to the item
            var reference = new ItemReferenceIndexNodeEntry(nodeId, itemId, position);
            this.AddNodeReferenceEntity(reference);

            // Add the reverse reference from the item to the index node, but only
            // if there isn't already a link in that direction
            var reverseReference = new ItemNodeIndexEntry(itemId, nodeId);
            this.AddItemNodeIndexEntry(reverseReference);
        }

        /// <summary>
        /// Removes an item entry.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        public void RemoveItemEntry(int itemId)
        {
            // First remove any remaining links this item has to any nodes
            this.RemoveAllItemNodeIndexEntries(itemId);

            // Remove the item itself from the index
            var pages = this.PageManager.ItemDataPages.FindPagesForEntry(itemId).ToArray();
            if (pages.Length > 1)
            {
                throw new PersistenceException("Expected only 1 page to contain an indexed item");
            }

            var removed = false;
            if (pages.Length == 1)
            {
                var page = (ItemIndexDataPage<TKey>)this.PageManager.GetPage(pages[0]);
                removed = page.RemoveEntry(e => e.Id == itemId);
                this.PageManager.SavePage(page);
            }

            if (!removed)
            {
                throw new PersistenceException("Unable to remove item entry because it doesn't exist");
            }

            this.itemIdLookup.Remove(this.itemLookup[itemId]);
            this.itemLookup.Remove(itemId);
        }

        /// <summary>
        /// Adds an item index entry.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="item">The item being added.</param>
        public void AddItemIndexEntry(int itemId, TKey item)
        {
            var pageHeader = this.PageManager.ItemDataPages.FindClosestPageForEntry(itemId);
            var page = (ItemIndexDataPage<TKey>)this.PageManager.GetPage(pageHeader);

            // Size = size of data + 4 bytes for internal id
            var itemEntry = new ItemEntry<TKey>(itemId, item, (short)(this.typePersistence.SizeReader(item) + 4));
            this.InsertEntry(itemEntry, page);

            this.itemIdLookup.Add(item, itemId);
            this.itemLookup.Add(itemId, item);
        }

        /// <summary>
        /// Restores all the items from the persisted file, creating lookups for the items
        /// keyed on both their internal ids and the index keys.
        /// </summary>
        public void Initialize()
        {
            // Load a lookup of all the serialized items from the backing store
            var itemEntries = this.GetAllItemEntries().ToList();

            // Create a lookup of ids keyed on the items themselves for future reference
            this.itemIdLookup = itemEntries.ToDictionary(e => e.Item, e => e.Id);

            // Create a lookup of the items keyed on their ids - this is used in the de-serialization process
            this.itemLookup = itemEntries.ToDictionary(e => e.Id, e => e.Item);
        }

        /// <summary>
        /// Performs a page split, copying any required items from the original page to the new page.
        /// </summary>
        /// <typeparam name="TPage">The type of the page.</typeparam>
        /// <typeparam name="TEntry">The type of the entry.</typeparam>
        /// <param name="id">The id of the item causing the page split.</param>
        /// <param name="page">The page that the new page should be created after.</param>
        /// <param name="item">The item being inserted that is responsible for the page split.</param>
        /// <returns>
        /// The page the item should be added to - this will be either the original page or the split page,
        /// depending on which is more empty.
        /// </returns>
        private TPage SplitPage<TPage, TEntry>(int id, TPage page, TEntry item)
            where TPage : DataPage<TEntry>
            where TEntry : IDataPageEntry
        {
            var splitPage = this.PageManager.CreatePage(page);
            if (page.Header.LastEntry <= id)
            {
                // Simple page split - just start inserting the new id on the new page
                page = splitPage;
            }
            else
            {
                // More complex - need to move items to the new page so the new entry can be inserted
                splitPage.MoveEntriesFrom(page, r => r.Id > id);

                // If the item still won't fit on either of the pages, make a final split for the item in its own right
                if (!page.CanContain(item) && !splitPage.CanContain(item))
                {
                    var finalSplitPage = this.PageManager.CreatePage(page);

                    // Persist the original page and first split pages now
                    this.PageManager.SavePage(page);
                    this.PageManager.SavePage(splitPage);

                    // Return the final split page - this is the one that will have the entry added to it
                    page = finalSplitPage;
                }
                else
                {
                    if (splitPage.Header.CurrentSize < page.Header.CurrentSize)
                    {
                        // Insert the entry into the split page - it currently contains less data
                        var tempPage = splitPage;
                        splitPage = page;
                        page = tempPage;
                    }

                    // Persist the split page now
                    this.PageManager.SavePage(splitPage);
                }
            }

            return page;
        }

        /// <summary>
        /// Inserts an entry into a page, causing a page split and moving data around if required.
        /// </summary>
        /// <typeparam name="TEntry">The type of the entry.</typeparam>
        /// <param name="entry">The entry to insert.</param>
        /// <param name="page">The page to try to insert the entry into.</param>
        private void InsertEntry<TEntry>(TEntry entry, DataPage<TEntry> page)
            where TEntry : IDataPageEntry
        {
            var id = entry.Id;
            var pageHeader = page.Header;
            if (!page.CanContain(entry))
            {
                // If the id fits after the end of the page, check to see if it can be inserted at the start of the next suitable page that has space
                var nextPage = page;
                while (nextPage.Header.NextPage != null && nextPage.Header.LastEntry <= id && !nextPage.CanContain(entry))
                {
                    nextPage = (DataPage<TEntry>)this.PageManager.GetPage(nextPage.Header.NextPage.GetValueOrDefault());
                }

                if (nextPage != page && nextPage.CanContain(entry))
                {
                    page = nextPage;
                }
                else
                {
                    // If the id fits before the start of the page, check to see if it can be inserted at the start of the previous page
                    var previousPage = pageHeader.PreviousPage != null && pageHeader.FirstEntry >= id ? (DataPage<TEntry>)this.PageManager.GetPage(pageHeader.PreviousPage.GetValueOrDefault()) : null;
                    if (previousPage != null && previousPage.CanContain(entry))
                    {
                        page = previousPage;
                    }
                    else
                    {
                        page = this.SplitPage(id, page, entry);
                    }
                }
            }

            page.AddEntry(entry);
            this.PageManager.SavePage(page);
        }

        /// <summary>
        /// Removes a node entry from the given page.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="nodeId">The referenced node id.</param>
        private void RemoveItemNodeIndexEntry(int itemId, int nodeId)
        {
            var pages = this.GetItemNodeIndexDataPages(itemId);
            foreach (var page in pages)
            {
                var removedFromPage = page.RemoveEntry(e => e.Id == itemId && e.ReferencedId == nodeId);
                if (removedFromPage)
                {
                    this.PageManager.SavePage(page);
                }
            }
        }

        /// <summary>
        /// Adds an item node index entry. This has no effect if there is already an entry with the
        /// same id and referencedId.
        /// </summary>
        /// <param name="reference">The reference to add.</param>
        private void AddItemNodeIndexEntry(ItemNodeIndexEntry reference)
        {
            var pageHeaders = this.PageManager.ItemNodeIndexDataPages.FindPagesForEntry(reference.Id).ToArray();
            if (pageHeaders.Length == 0)
            {
                // This is the first entry for this id.
                var pageHeader = this.PageManager.ItemNodeIndexDataPages.FindClosestPageForEntry(reference.Id);
                var page = (ItemNodeIndexDataPage)this.PageManager.GetPage(pageHeader);
                this.InsertEntry(reference, page);
            }
            else
            {
                ItemNodeIndexDataPage smallestPage = null;
                foreach (var pageHeader in pageHeaders)
                {
                    var page = (ItemNodeIndexDataPage)this.PageManager.GetPage(pageHeader);
                    if (smallestPage == null || pageHeader.CurrentSize < smallestPage.Header.CurrentSize)
                    {
                        smallestPage = page;
                    }

                    foreach (var entry in page.Entries)
                    {
                        if (entry.Id == reference.Id && entry.ReferencedId == reference.ReferencedId)
                        {
                            // Short-circuit - entry already exists.
                            return;
                        }
                    }
                }

                // Until we have a smarter indexing approach for referenced ids, just insert into the smallest page
                this.InsertEntry(reference, smallestPage);
            }
        }

        /// <summary>
        /// Removes all item node entries from both sides of the relationship.
        /// </summary>
        /// <param name="itemId">The item id for which to remove all the index entries.</param>
        private void RemoveAllItemNodeIndexEntries(int itemId)
        {
            var pages = this.GetItemNodeIndexDataPages(itemId);
            foreach (var page in pages)
            {
                var pageContainsEntries = false;
                bool SearchPredicate(ItemNodeIndexEntry e) => e.Id == itemId;
                foreach (var entry in page.Entries.Where(SearchPredicate))
                {
                    pageContainsEntries = true;
                    this.RemoveIndexNodeEntry(entry.ReferencedId, entry.Id, IndexNodeEntryType.ItemReference);
                }

                if (pageContainsEntries)
                {
                    page.RemoveEntry(SearchPredicate);
                    this.PageManager.SavePage(page);
                }
            }
        }

        /// <summary>
        /// Removes a node entry from the given page.
        /// </summary>
        /// <param name="id">The id of the node to remove the entry for.</param>
        /// <param name="referencedId">The referenced id to remove.</param>
        /// <param name="entryType">The type of the entry to remove.</param>
        private void RemoveIndexNodeEntry(int id, int referencedId, IndexNodeEntryType entryType)
        {
            var pages = this.GetIndexNodeDataPages(id);
            var removed = false;
            foreach (var page in pages)
            {
                var removedFromPage = page.RemoveEntry(e => e.Id == id && e.ReferencedId == referencedId && e.EntryType == entryType);
                removed |= removedFromPage;

                if (removedFromPage)
                {
                    this.PageManager.SavePage(page);

                    if (entryType == IndexNodeEntryType.NodeReference)
                    {
                        // A node reference can only appear once, so break out if it is encountered
                        // Item references can appear multiple times across any of the pages
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a node reference.
        /// </summary>
        /// <param name="reference">The reference to add.</param>
        private void AddNodeReferenceEntity(IndexNodeEntryBase reference)
        {
            // Find the page the new reference should be added to
            var pageHeader = this.PageManager.IndexNodeDataPages.FindClosestPageForEntry(reference.Id);
            var page = (IndexNodeDataPage)this.PageManager.GetPage(pageHeader);

            this.InsertEntry(reference, page);
        }

        /// <summary>
        /// Gets the index node data pages that contain entries for the given id.
        /// </summary>
        /// <param name="logicalId">The logical id of the node.</param>
        /// <returns>The relevant pages.</returns>
        private IEnumerable<IndexNodeDataPage> GetIndexNodeDataPages(int logicalId)
        {
            foreach (var pageHeader in this.PageManager.IndexNodeDataPages.FindPagesForEntry(logicalId))
            {
                yield return (IndexNodeDataPage)this.PageManager.GetPage(pageHeader);
            }
        }

        /// <summary>
        /// Gets the index node data pages that contain entries for the given id.
        /// </summary>
        /// <param name="logicalId">The logical id of the item.</param>
        /// <returns>The relevant pages.</returns>
        private IEnumerable<ItemNodeIndexDataPage> GetItemNodeIndexDataPages(int logicalId)
        {
            foreach (var pageHeader in this.PageManager.ItemNodeIndexDataPages.FindPagesForEntry(logicalId))
            {
                yield return (ItemNodeIndexDataPage)this.PageManager.GetPage(pageHeader);
            }
        }
    }
}

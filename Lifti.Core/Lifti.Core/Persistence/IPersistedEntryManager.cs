// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The interface implemented by classes capable of managing individual entries in the persisted file store. These entries can be
    /// representative of items that have been indexed in the full text index, and information about how the index's nodes are 
    /// structured.
    /// </summary>
    /// <typeparam name="TKey">The type of the key stored in the index.</typeparam>
    public interface IPersistedEntryManager<TKey> : IDisposable
    {
        /// <summary>
        /// Gets the page manager the persisted entry manager is responsible for.
        /// </summary>
        /// <value>The associated <see cref="IPageManager"/> instance.</value>
        IPageManager PageManager
        {
            get;
        }

        /// <summary>
        /// Gets the number of items stored in the persisted backing store.
        /// </summary>
        /// <value>The number of items stored in the index.</value>
        int ItemCount { get; }

        /// <summary>
        /// Gets all the item entries held in the index.
        /// </summary>
        /// <returns>All the item entries.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is a non-trivial operation, and should be a method.")]
        IEnumerable<ItemEntry<TKey>> GetAllItemEntries();

        /// <summary>
        /// Gets the index node entries, both <see cref="ItemReferenceIndexNodeEntry"/> and <see cref="NodeReferenceIndexNodeEntry"/>, for the
        /// node with the given logical id.
        /// </summary>
        /// <param name="logicalId">The logical id of the node to get the entries for.</param>
        /// <returns>An enumerable set of <see cref="IndexNodeEntryBase"/> that may be of type <see cref="ItemReferenceIndexNodeEntry"/> or <see cref="NodeReferenceIndexNodeEntry"/>.</returns>
        IEnumerable<IndexNodeEntryBase> GetIndexNodeEntries(int logicalId);

        /// <summary>
        /// Adds an index node reference entry.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="nodeId">The node id.</param>
        /// <param name="indexedCharacter">The indexed character.</param>
        void AddIndexNodeReferenceEntry(int parentId, int nodeId, char indexedCharacter);

        /// <summary>
        /// Removes an index node reference entry.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="nodeId">The node id.</param>
        void RemoveIndexNodeReferenceEntry(int parentId, int nodeId);

        /// <summary>
        /// Removes a node item entry.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <param name="itemId">The item id.</param>
        void RemoveNodeItemEntry(int nodeId, int itemId);

        /// <summary>
        /// Adds a node item entry.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <param name="itemId">The item id.</param>
        /// <param name="position">The word position.</param>
        void AddNodeItemEntry(int nodeId, int itemId, int position);

        /// <summary>
        /// Removes an item index entry.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        void RemoveItemEntry(int itemId);

        /// <summary>
        /// Adds an item index entry.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="item">The item being added.</param>
        void AddItemIndexEntry(int itemId, TKey item);

        /// <summary>
        /// Gets the internal id of the given item.
        /// </summary>
        /// <param name="item">The item to get the id for.</param>
        /// <returns>The internal id of the item.</returns>
        int GetIdForItem(TKey item);

        /// <summary>
        /// Gets the item associated to an internal id.
        /// </summary>
        /// <param name="itemId">The if of the item to get.</param>
        /// <returns>
        /// The internal id of the item.
        /// </returns>
        TKey GetItemForId(int itemId);

        /// <summary>
        /// Prepares the entry manager for use - this onlu needs to be called once, and is generally done
        /// by the full text index instance when it is constructed.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets a value indicating whether the given item is contained within the index.
        /// </summary>
        /// <param name="item">The item to check the existence of.</param>
        /// <returns><c>true</c> if the item is contained in the index, otherwise <c>false</c>.</returns>
        bool ItemIndexed(TKey item);
    }
}

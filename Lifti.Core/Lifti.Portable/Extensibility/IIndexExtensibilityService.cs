// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Extensibility
{
    #region Using statements

    using System;

    #endregion

    /// <summary>
    /// The interface implemented by a full text index's extensibility service.
    /// </summary>
    /// <typeparam name="TKey">The type of the key stored in the full text index the service is associated to.</typeparam>
    public interface IIndexExtensibilityService<TKey>
    {
        /// <summary>
        /// Occurs when an item is about to be indexed in the full text index. This event is fired once per item, 
        /// regardless of how many words it is associated to.
        /// </summary>
        event EventHandler<ItemEventArgs<TKey>> ItemIndexingStarted;

        /// <summary>
        /// Occurs when all the text associated to an item has been indexed in the full text index.
        /// </summary>
        event EventHandler<ItemEventArgs<TKey>> ItemIndexingCompleted;

        /// <summary>
        /// Occurs when an item is about to be removed from the index.
        /// </summary>
        event EventHandler<ItemEventArgs<TKey>> ItemRemovalStarted;

        /// <summary>
        /// Occurs when an item has been completely removed from the index.
        /// </summary>
        event EventHandler<ItemEventArgs<TKey>> ItemRemovalCompleted;

        /// <summary>
        /// Occurs when an item's word has been indexed in the full text index. This event's arguments provides both the 
        /// item that was indexed and the node it was indexed against.
        /// </summary>
        event EventHandler<ItemWordIndexedEventArgs<TKey>> ItemWordIndexed;

        /// <summary>
        /// Occurs when a word associated to an item is removed from the full text index. This event's arguments provides both the 
        /// item that was indexed and the node that used to contain the reference to it.
        /// </summary>
        event EventHandler<ItemNodeEventArgs<TKey>> ItemWordRemoved;

        /// <summary>
        /// Occurs when a node is created in the full text index.
        /// </summary>
        event EventHandler<IndexNodeEventArgs<TKey>> NodeCreated;

        /// <summary>
        /// Occurs when a node is invalidated in a persisted full text index. Node invalidation occurs when there is a chance
        /// that the in-memory representation of the index is out of sync with the persisted version. An invalidated node will be
        /// re-loaded the next time it is accessed.
        /// </summary>
        event EventHandler<IndexNodeEventArgs<TKey>> NodeInvalidated;

        /// <summary>
        /// Occurs when a node is restored in a persisted full text index. Node restoration occurs when a node has been
        /// loaded from the backing file store.
        /// </summary>
        event EventHandler<IndexNodeEventArgs<TKey>> NodeRestored;

        /// <summary>
        /// Occurs when a node is removed from the full text index.
        /// </summary>
        event EventHandler<IndexNodeEventArgs<TKey>> NodeRemoved;

        /// <summary>
        /// Adds the specified add-in into the extensibility service.
        /// </summary>
        /// <param name="name">The name to register the add-in as.</param>
        /// <param name="addIn">The add-in to add.</param>
        void Add(string name, IIndexAddIn<TKey> addIn);

        /// <summary>
        /// Removes the add-in with the given name from the extensibility service.
        /// </summary>
        /// <param name="name">The registered name of the add-in to remove.</param>
        void Remove(string name);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.ItemIndexingStarted"/> event.
        /// </summary>
        /// <param name="item">The item for which indexing has started for.</param>
        void OnItemIndexingStarted(TKey item);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.ItemIndexingCompleted" /> event.
        /// </summary>
        /// <param name="item">The item for which indexing has completed for.</param>
        void OnItemIndexingCompleted(TKey item);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.ItemRemovalStarted" /> event.
        /// </summary>
        /// <param name="item">The item for which removal has started for.</param>
        void OnItemRemovalStarted(TKey item);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.ItemRemovalCompleted" /> event.
        /// </summary>
        /// <param name="item">The item for which removal has completed for.</param>
        void OnItemRemovalCompleted(TKey item);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.ItemWordIndexed" /> event.
        /// </summary>
        /// <param name="itemWordMatch">The item word match for which the word has been indexed.</param>
        /// <param name="node">The node the item was indexed against.</param>
        void OnItemWordIndexed(ItemWordMatch<TKey> itemWordMatch, IndexNode<TKey> node);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.ItemWordRemoved" /> event.
        /// </summary>
        /// <param name="item">The item for which the indexed word has been removed.</param>
        /// <param name="node">The node the item was indexed against.</param>
        void OnItemWordRemoved(TKey item, IndexNode<TKey> node);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.NodeCreated" /> event.
        /// </summary>
        /// <param name="node">The node that has been created in the index.</param>
        void OnNodeCreated(IndexNode<TKey> node);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.NodeRemoved" /> event.
        /// </summary>
        /// <param name="node">The node that has been removed from the index.</param>
        void OnNodeRemoved(IndexNode<TKey> node);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.NodeInvalidated" /> event.
        /// </summary>
        /// <param name="node">The node that has been invalidated in the index.</param>
        void OnNodeInvalidating(IndexNode<TKey> node);

        /// <summary>
        /// Raises the <see cref="IIndexExtensibilityService&lt;TKey&gt;.NodeRestored" /> event.
        /// </summary>
        /// <param name="node">The node that has been restored in the index.</param>
        void OnNodeRestored(IndexNode<TKey> node);
    }
}
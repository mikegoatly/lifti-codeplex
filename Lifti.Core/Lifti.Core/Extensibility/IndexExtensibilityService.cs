// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Extensibility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The implementation of the full text index's extensibility service. This is the communication hub through which plug-ins can 
    /// register themselves and respond to various events raised during the lifetime of a full text index.
    /// </summary>
    /// <typeparam name="TKey">The type of the key stored in the full text index the service is associated to.</typeparam>
    public class IndexExtensibilityService<TKey> : IIndexExtensibilityService<TKey>
    {
        /// <summary>
        /// The list of registered add-ins, keyed on the name they were registered against.
        /// </summary>
        private readonly Dictionary<string, IIndexAddIn<TKey>> registeredAddins = new Dictionary<string, IIndexAddIn<TKey>>();

        /// <inheritdoc />
        public event EventHandler<ItemEventArgs<TKey>> ItemIndexingStarted;

        /// <inheritdoc />
        public event EventHandler<ItemEventArgs<TKey>> ItemIndexingCompleted;

        /// <inheritdoc />
        public event EventHandler<ItemEventArgs<TKey>> ItemRemovalStarted;

        /// <inheritdoc />
        public event EventHandler<ItemEventArgs<TKey>> ItemRemovalCompleted;

        /// <inheritdoc />
        public event EventHandler<ItemWordIndexedEventArgs<TKey>> ItemWordIndexed;

        /// <inheritdoc />
        public event EventHandler<ItemNodeEventArgs<TKey>> ItemWordRemoved;

        /// <inheritdoc />
        public event EventHandler<IndexNodeEventArgs<TKey>> NodeCreated;

        /// <inheritdoc />
        public event EventHandler<IndexNodeEventArgs<TKey>> NodeRemoved;

        /// <inheritdoc />
        public event EventHandler<IndexNodeEventArgs<TKey>> NodeInvalidated;

        /// <inheritdoc />
        public event EventHandler<IndexNodeEventArgs<TKey>> NodeRestored;

        /// <inheritdoc />
        public void Add(string name, IIndexAddIn<TKey> addIn)
        {
            if (addIn == null)
            {
                throw new ArgumentNullException(nameof(addIn));
            }

            if (this.registeredAddins.ContainsKey(name))
            {
                throw new ArgumentException("An add-in has already been registered with the given name", nameof(name));
            }

            addIn.Initialize(this);
            this.registeredAddins.Add(name, addIn);
        }

        /// <inheritdoc />
        public void Remove(string name)
        {
            this.registeredAddins.Remove(name);
        }

        /// <inheritdoc />
        public void OnItemIndexingStarted(TKey item)
        {
            this.ItemIndexingStarted?.Invoke(this, new ItemEventArgs<TKey>(item));
        }

        /// <inheritdoc />
        public void OnItemIndexingCompleted(TKey item)
        {
            this.ItemIndexingCompleted?.Invoke(this, new ItemEventArgs<TKey>(item));
        }

        /// <inheritdoc />
        public void OnItemRemovalStarted(TKey item)
        {
            this.ItemRemovalStarted?.Invoke(this, new ItemEventArgs<TKey>(item));
        }

        /// <inheritdoc />
        public void OnItemRemovalCompleted(TKey item)
        {
            this.ItemRemovalCompleted?.Invoke(this, new ItemEventArgs<TKey>(item));
        }

        /// <inheritdoc />
        public void OnItemWordIndexed(ItemWordMatch<TKey> itemWordMatch, IndexNode<TKey> node)
        {
            this.ItemWordIndexed?.Invoke(this, new ItemWordIndexedEventArgs<TKey>(node, itemWordMatch));
        }

        /// <inheritdoc />
        public void OnItemWordRemoved(TKey item, IndexNode<TKey> node)
        {
            this.ItemWordRemoved?.Invoke(this, new ItemNodeEventArgs<TKey>(node, item));
        }

        /// <inheritdoc />
        public void OnNodeCreated(IndexNode<TKey> node)
        {
            this.NodeCreated?.Invoke(this, new IndexNodeEventArgs<TKey>(node));
        }

        /// <inheritdoc />
        public void OnNodeRemoved(IndexNode<TKey> node)
        {
            this.NodeRemoved?.Invoke(this, new IndexNodeEventArgs<TKey>(node));
        }

        /// <inheritdoc />
        public void OnNodeInvalidating(IndexNode<TKey> node)
        {
            this.NodeInvalidated?.Invoke(this, new IndexNodeEventArgs<TKey>(node));
        }

        /// <inheritdoc />
        public void OnNodeRestored(IndexNode<TKey> node)
        {
            this.NodeRestored?.Invoke(this, new IndexNodeEventArgs<TKey>(node));
        }
    }
}

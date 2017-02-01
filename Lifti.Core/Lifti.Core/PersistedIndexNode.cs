// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System.Collections.Generic;
    using System.Linq;

    using Lifti.Persistence;

    /// <summary>
    /// The type of index node contained within a persisted full text index. This adds in functionality to
    /// lazy-load the index from the backing store.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class PersistedIndexNode<TKey> : IndexNode<TKey>
    {
        /// <summary>
        /// The thread synchronization object.
        /// </summary>
        private readonly object syncObj = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedIndexNode{TKey}"/> class.
        /// </summary>
        /// <param name="index">The index the node is a part of.</param>
        /// <param name="indexedCharacter">The indexed character.</param>
        /// <param name="internalId">The internal id of the node as defined in the backing store.</param>
        public PersistedIndexNode(IFullTextIndex<TKey> index, char indexedCharacter, int internalId)
            : base(index, indexedCharacter)
        {
            this.InternalId = internalId;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PersistedIndexNode&lt;TKey&gt;"/> is populated
        /// from the backing store or not. When the node is invalidated this will be set to <c>false</c>, when it is
        /// restored it will be set to <c>true</c>. A node that is created as new will default to <c>true</c>.
        /// </summary>
        /// <value><c>true</c> if this instance has been populated; otherwise, <c>false</c>.</value>
        public bool Populated
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the internal id of the node.
        /// </summary>
        public int InternalId
        {
            get;
        }

        /// <summary>
        /// Gets or sets the child nodes under this one, indexed by the next character in the word.
        /// </summary>
        /// <value>The child nodes.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This needs to be settable by deriving classes to support lazy loading")]
        protected sealed override Dictionary<char, IndexNode<TKey>> ChildNodes
        {
            get
            {
                this.LazyLoadIfRequired();

                return base.ChildNodes;
            }

            set
            {
                base.ChildNodes = value;
            }
        }

        /// <summary>
        /// Gets or sets the items that match a word that is completed at this instance.
        /// </summary>
        /// <value>The list of items.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This needs to be settable by deriving classes to support lazy loading")]
        protected sealed override Dictionary<TKey, ItemWordMatch<TKey>> Items
        {
            get
            {
                this.LazyLoadIfRequired();

                return base.Items;
            }

            set
            {
                base.Items = value;
            }
        }

        /// <summary>
        /// Invalidates this instance, causing the node's data to be loaded from the persisted backing store
        /// when it is next accessed.
        /// </summary>
        public void Invalidate()
        {
            lock (this.syncObj)
            {
                if (this.Populated)
                {
                    this.Index.Extensibility.OnNodeInvalidating(this);

                    if (this.ChildNodes != null)
                    {
                        foreach (var node in this.ChildNodes.Values.Cast<PersistedIndexNode<TKey>>())
                        {
                            node.Invalidate();
                        }

                        this.ChildNodes.Clear();
                        this.ChildNodes = null;
                    }

                    if (this.Items != null)
                    {
                        this.Items.Clear();
                        this.Items = null;
                    }

                    this.Populated = false;
                }
            }
        }

        /// <summary>
        /// Checks to see if this node is populated. If it isn't then it is lazy-loaded from the backing
        /// store.
        /// </summary>
        private void LazyLoadIfRequired()
        {
            if (!this.Populated)
            {
                lock (this.syncObj)
                {
                    if (!this.Populated)
                    {
                        this.LoadFromStore();
                    }
                }
            }
        }

        /// <summary>
        /// Loads the child items and nodes from the persisted file store.
        /// </summary>
        private void LoadFromStore()
        {
            var entryManager = ((PersistedFullTextIndex<TKey>)this.Index).PersistedEntryManager;
            var nodeEntries = entryManager.GetIndexNodeEntries(this.InternalId).ToList();

            this.LoadChildNodes(nodeEntries);
            this.LoadChildItems(entryManager, nodeEntries);

            this.Populated = true;

            this.Index.Extensibility.OnNodeRestored(this);
        }

        /// <summary>
        /// Loads the child nodes from the persisted node entries.
        /// </summary>
        /// <param name="nodeEntries">The node entries to restore this instance's child nodes from.</param>
        private void LoadChildNodes(IEnumerable<IndexNodeEntryBase> nodeEntries)
        {
            var childNodes = nodeEntries.OfType<NodeReferenceIndexNodeEntry>()
                .Select(r => new PersistedIndexNode<TKey>(this.Index, r.MatchedCharacter, r.ReferencedId))
                .Cast<IndexNode<TKey>>()
                .ToDictionary(c => c.IndexedCharacter);

            if (childNodes.Count > 0)
            {
                foreach (var childNode in childNodes.Values)
                {
                    childNode.Parent = this;
                }
                     
                this.ChildNodes = childNodes;
            }
        }

        /// <summary>
        /// Loads the child items from the persisted node entries.
        /// </summary>
        /// <param name="entryManager">The entry manager capable of getting an item for an internal item id.</param>
        /// <param name="nodeEntries">The node entries to restore this instance's items from.</param>
        private void LoadChildItems(IPersistedEntryManager<TKey> entryManager, IEnumerable<IndexNodeEntryBase> nodeEntries)
        {
            // Get all the items in the node, grouped by the referenced id 
            // (one item will be referenced multiple times if the same word appears against it multiple locations)
            var items = nodeEntries.OfType<ItemReferenceIndexNodeEntry>()
                .GroupBy(r => r.ReferencedId)
                .Select(r => new ItemWordMatch<TKey>(
                                 entryManager.GetItemForId(r.Key),
                                 r.Select(g => g.MatchPosition).OrderBy(p => p).ToArray())).ToDictionary(p => p.Item);

            if (items.Count > 0)
            {
                this.Items = items;
            }
        }
    }
}

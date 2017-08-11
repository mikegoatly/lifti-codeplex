// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System;
    using System.Collections.Generic;

    using Lifti.Extensibility;
    using Lifti.Persistence;
    using Lifti.Persistence.IO;
    using System.IO;

    /// <summary>
    /// An updatable full text index that keeps a persisted file in-sync with the index, meaning that
    /// it automatically starts up in the state it was last used in.
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the index.</typeparam>
    /// <example>
    /// <code><![CDATA[
    /// private PersistedFullTextIndex<string> index;
    /// 
    /// void Main()
    /// {
    ///     var indexFilePath = Path.Combine(
    ///         Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    ///         Path.Combine("MyApplication", "MyTextIndex.dat");
    /// 
    ///     // If the file doesn't exist then it will be created, otherwise its
    ///     // current state will be restored
    ///     this.index = new PersistedFullTextIndex<string>(indexFilePath)
    ///     {
    ///         WordSplitter = new StemmingWordSplitter(),
    ///         QueryParser = new LiftiQueryParser()
    ///     };
    /// 
    ///     // Only index documents that haven't already been indexed
    ///     var files = System.IO.Directory.EnumerateFiles(@"c:\MyDocumentStore")
    ///         .Where(f => !this.index.Contains(f));
    /// 
    ///     this.index.Index(files, f => GetDocumentText(f));
    /// 
    ///     Console.WriteLine("Searching for documents containing the words 'design' and 'document'");
    ///     foreach (var fileName in this.index.Search("design & document"))
    ///     {
    ///         Console.WriteLine("Document file name:", fileName);
    ///     }
    /// }
    /// 
    /// private string GetDocumentText(string filename)
    /// {
    ///     return File.ReadAllText(filename);
    /// }
    /// ]]></code>
    /// </example>
    public class PersistedFullTextIndex<TKey> : UpdatableFullTextIndex<TKey>
    {
        /// <summary>
        /// This contains the ids of nodes that have been removed from the index. These can be safely
        /// re-used to make it less likely that the upper bounds of the id range are encountered.
        /// </summary>
        private readonly Stack<int> reusableIndexNodeIds = new Stack<int>();

        /// <summary>
        /// This contains the ids of the items that have been removed from the index. These can be safely
        /// re-used to make it less likely that the upper bounds of the id range are encountered.
        /// </summary>
        private readonly Stack<int> reusableItemIds = new Stack<int>();

        /// <inheritdoc />
        public PersistedFullTextIndex(Stream backingFileStream)
            : this(backingFileStream, InferTypePersistence())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedFullTextIndex&lt;TKey&gt;"/> class.
        /// </summary>
        /// <param name="backingFileStream">The stream for the backing file.</param>
        /// <param name="typePersistence">The type persistence instance capable of reading/writing TKey instances to a binary reader or writer.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "These live for the lifetime of the object and are disposed when this instance is")]
        public PersistedFullTextIndex(Stream backingFileStream, ITypePersistence<TKey> typePersistence)
            : this(new DataFileManager(backingFileStream), typePersistence, new IndexExtensibilityService<TKey>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedFullTextIndex&lt;TKey&gt;"/> class.
        /// </summary>
        /// <param name="persistedEntryManager">The persisted entry manager for the instance.</param>
        /// <param name="extensibilityService">The extensibility service.</param>
        public PersistedFullTextIndex(IPersistedEntryManager<TKey> persistedEntryManager, IIndexExtensibilityService<TKey> extensibilityService)
            : base(extensibilityService)
        {
            this.PersistedEntryManager = persistedEntryManager ?? throw new ArgumentNullException(nameof(persistedEntryManager));
            this.PersistedEntryManager.Initialize();

            this.Extensibility.Add("IndexPersistence", new PersistenceAddIn(this));

            this.RootNode = new PersistedIndexNode<TKey>(this, '\0', 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedFullTextIndex&lt;TKey&gt;"/> class.
        /// </summary>
        /// <param name="dataFileManager">The data file manager.</param>
        /// <param name="typePersistence">The type persistence.</param>
        /// <param name="extensibilityService">The extensibility service.</param>
        private PersistedFullTextIndex(IDataFileManager dataFileManager, ITypePersistence<TKey> typePersistence, IIndexExtensibilityService<TKey> extensibilityService)
            : this(
                new BufferedPageManager<TKey>(new PageCache(), new PersistenceSettings(), dataFileManager, typePersistence, extensibilityService),
                typePersistence,
                extensibilityService)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedFullTextIndex&lt;TKey&gt;"/> class.
        /// </summary>
        /// <param name="pageManager">The page manager.</param>
        /// <param name="typePersistence">The type persistence.</param>
        /// <param name="extensibilityService">The extensibility service.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "These live for the lifetime of the object and are disposed when this instance is")]
        private PersistedFullTextIndex(IPageManager pageManager, ITypePersistence<TKey> typePersistence, IIndexExtensibilityService<TKey> extensibilityService)
            : this(new PersistedEntryManager<TKey>(pageManager, typePersistence), extensibilityService)
        {
        }

        /// <summary>
        /// Gets the entry manager for the backing file store.
        /// </summary>
        public IPersistedEntryManager<TKey> PersistedEntryManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new node that will be contained in the index.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>The created node.</returns>
        /// <remarks><para>The creation of new node instance is generally managed by the LIFTI framework, and you
        /// probably won't need to use this method.</para>
        /// <para>This method can be overridden by deriving classes to allow for a different derivative 
        /// of <see cref="IndexNode{TKey}"/> to be stored in the index.</para></remarks>
        public override IndexNode<TKey> CreateIndexNode(char character)
        {
            return new PersistedIndexNode<TKey>(this, character, this.ReuseOrObtainItemIndexNodeId())
            {
                // Mark the node as populated - there cannot be anything in the backing store
                // for this node as it is new, so this saves a lookup
                Populated = true
            };
        }

        /// <summary>
        /// Determines whether the specified key exists in the index.
        /// </summary>
        /// <param name="item">The item key to check for.</param>
        /// <returns>
        ///   <c>true</c> if the index contains the specified item key; otherwise, <c>false</c>.
        /// </returns>
        protected override bool ContainsKey(TKey item)
        {
            // The item may not have been loaded into any nodes in the tree, so a check should
            // be made against the entry manager, which maintains the list of all the indexed items
            return this.PersistedEntryManager.ItemIndexed(item);
        }

        /// <summary>
        /// Determines the number of items currently held in the index.
        /// </summary>
        /// <returns>The number of items in the index.</returns>
        protected override int DetermineItemCount()
        {
            // All the items may not have been loaded into the index tree, so
            // the entry manager is the place to get the current item count from.
            return this.PersistedEntryManager.ItemCount;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (this.PersistedEntryManager != null)
            {
                this.PersistedEntryManager.Dispose();
                this.PersistedEntryManager = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Infers the type persistence for TKey.
        /// </summary>
        /// <returns>The relevant <see cref="ITypePersistence&lt;TKey&gt;"/> instance.</returns>
        private static ITypePersistence<TKey> InferTypePersistence()
        {
            var keyType = typeof(TKey);
            if (keyType == typeof(string))
            {
                return (ITypePersistence<TKey>)new StringPersistence();
            }

            return new GenericPersistence<TKey>();
        }

        /// <summary>
        /// Gets a reused or newly obtained item index node internal id.
        /// </summary>
        /// <returns>The id to use for the new index node.</returns>
        private int ReuseOrObtainItemIndexNodeId()
        {
            if (this.reusableIndexNodeIds.Count > 0)
            {
                return this.reusableIndexNodeIds.Pop();
            }

            return this.PersistedEntryManager.PageManager.AllocateNewIndexNodeId();
        }

        /// <summary>
        /// Gets a reused or newly obtained item internal id.
        /// </summary>
        /// <returns>The id to use for the new item.</returns>
        private int ReuseOrObtainItemId()
        {
            if (this.reusableItemIds.Count > 0)
            {
                return this.reusableItemIds.Pop();
            }

            return this.PersistedEntryManager.PageManager.AllocateNewItemId();
        }

        /// <summary>
        /// The add-in that acts as a bridge between the index and the underlying persisted file store.
        /// </summary>
        private class PersistenceAddIn : IIndexAddIn<TKey>
        {
            /// <summary>
            /// The index the add-in is handling the interactions for.
            /// </summary>
            private readonly PersistedFullTextIndex<TKey> index;

            /// <summary>
            /// The thread synchronization object used to synchronize access to methods involved
            /// in lazy loading. Lazy loading can occur on multiple reader threads and can lead to 
            /// race conditions if not serialized.
            /// </summary>
            private readonly object lazyLoadSyncObj = new object();

            /// <summary>
            /// Initializes a new instance of the <see cref="PersistedFullTextIndex&lt;TKey&gt;.PersistenceAddIn"/> class.
            /// </summary>
            /// <param name="index">The index the add-in is handling the interactions for.</param>
            public PersistenceAddIn(PersistedFullTextIndex<TKey> index)
            {
                this.index = index;
            }

            /// <summary>
            /// Initializes this instance, allowing the add-in code to hook into the required
            /// extensibility points.
            /// </summary>
            /// <param name="extensibilityService">The extensibility service.</param>
            public void Initialize(IIndexExtensibilityService<TKey> extensibilityService)
            {
                if (extensibilityService == null)
                {
                    throw new ArgumentNullException(nameof(extensibilityService));
                }

                extensibilityService.ItemWordIndexed += this.OnItemWordIndexed;
                extensibilityService.ItemWordRemoved += this.OnItemWordRemoved;
                extensibilityService.NodeCreated += this.OnNodeCreated;
                extensibilityService.NodeRemoved += this.OnNodeRemoved;
                extensibilityService.NodeInvalidated += this.OnNodeInvalidated;
                extensibilityService.NodeRestored += this.OnNodeRestored;
                extensibilityService.ItemIndexingStarted += this.OnItemIndexingStarted;
                extensibilityService.ItemIndexingCompleted += this.OnItemIndexingCompleted;
                extensibilityService.ItemRemovalCompleted += this.OnItemRemovalCompleted;
            }

            /// <summary>
            /// Called when indexing of an item has started.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.ItemEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnItemIndexingStarted(object sender, ItemEventArgs<TKey> e)
            {
                var item = e.Item;

                // Add a reference to the item in the persisted file
                var itemId = this.index.ReuseOrObtainItemId();
                this.index.PersistedEntryManager.AddItemIndexEntry(itemId, item);
            }

            /// <summary>
            /// Called when indexing of an item has completed.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.ItemEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnItemIndexingCompleted(object sender, ItemEventArgs<TKey> e)
            {
                this.index.PersistedEntryManager.PageManager.Flush();
            }

            /// <summary>
            /// Called when an item has been completely removed from the index.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.ItemEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnItemRemovalCompleted(object sender, ItemEventArgs<TKey> e)
            {
                // Unfortunately there's currently no easy way to verify that all references to the item have been removed from the
                // backing store before removing the item entry in the index...
                var item = e.Item;
                var itemId = this.GetItemId(item);

                this.index.PersistedEntryManager.RemoveItemEntry(itemId);
                this.index.reusableItemIds.Push(itemId);

                this.index.PersistedEntryManager.PageManager.Flush();
            }

            /// <summary>
            /// Called when a node is invalidated in the index.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.IndexNodeEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnNodeInvalidated(object sender, IndexNodeEventArgs<TKey> e)
            {
                lock (this.lazyLoadSyncObj)
                {
                    // Make sure the underlying updatable index is aware that all the items are no longer recorded against the
                    // invalidated node
                    var node = e.Node;
                    foreach (var itemMatch in node.GetDirectItems())
                    {
                        this.index.OnItemWordRemoved(itemMatch.Item, node);
                    }
                }
            }

            /// <summary>
            /// Called when a node is restored in the index.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.IndexNodeEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnNodeRestored(object sender, IndexNodeEventArgs<TKey> e)
            {
                // Only allow one node to be restored at a time - this prevents multiple readers encountering
                // race conditions when lazy-loading data.
                lock (this.lazyLoadSyncObj)
                {
                    // Make sure the underlying updatable index is aware that all the items are recorded against the
                    // restored node
                    var node = e.Node;
                    foreach (var itemMatch in node.GetDirectItems())
                    {
                        this.index.OnItemWordIndexed(itemMatch.Item, node);
                    }
                }
            }

            /// <summary>
            /// Called when a node is created in the index.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.IndexNodeEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnNodeCreated(object sender, IndexNodeEventArgs<TKey> e)
            {
                var persistedNode = e.Node as PersistedIndexNode<TKey>;
                if (persistedNode == null)
                {
                    throw new PersistenceException("Node is not of type Persisted Index Node.");
                }

                var parent = (PersistedIndexNode<TKey>)persistedNode.Parent;
                var parentId = parent.InternalId;

                this.index.PersistedEntryManager.AddIndexNodeReferenceEntry(
                    parentId,
                    persistedNode.InternalId,
                    persistedNode.IndexedCharacter);
            }

            /// <summary>
            /// Called when a node is removed from the index.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.IndexNodeEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnNodeRemoved(object sender, IndexNodeEventArgs<TKey> e)
            {
                var persistedNode = e.Node as PersistedIndexNode<TKey>;
                if (persistedNode == null)
                {
                    throw new PersistenceException("Node is not of type Persisted Index Node.");
                }

                var nodeId = persistedNode.InternalId;
                var parent = (PersistedIndexNode<TKey>)persistedNode.Parent;
                var parentId = parent.InternalId;

                this.index.PersistedEntryManager.RemoveIndexNodeReferenceEntry(parentId, nodeId);

                this.index.reusableIndexNodeIds.Push(nodeId);
            }

            /// <summary>
            /// Called when a word is removed from an item in the index.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.ItemNodeEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnItemWordRemoved(object sender, ItemNodeEventArgs<TKey> e)
            {
                var persistedNode = e.Node as PersistedIndexNode<TKey>;
                if (persistedNode == null)
                {
                    throw new PersistenceException("Node is not of type Persisted Index Node.");
                }

                var itemId = this.GetItemId(e.Item);

                this.index.PersistedEntryManager.RemoveNodeItemEntry(persistedNode.InternalId, itemId);
            }

            /// <summary>
            /// Gets the item id for the given item.
            /// </summary>
            /// <param name="item">The item to get the internal id for.</param>
            /// <returns>The internal id for the given item</returns>
            private int GetItemId(TKey item)
            {
                return this.index.PersistedEntryManager.GetIdForItem(item);
            }

            /// <summary>
            /// Called when a word is added to the index for an item.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The <see cref="Lifti.Extensibility.ItemWordIndexedEventArgs&lt;TKey&gt;"/> instance containing the event data.</param>
            private void OnItemWordIndexed(object sender, ItemWordIndexedEventArgs<TKey> e)
            {
                var persistedNode = e.Node as PersistedIndexNode<TKey>;
                if (persistedNode == null)
                {
                    throw new PersistenceException("Node is not of type Persisted Index Node.");
                }

                var itemMatch = e.ItemWordMatch;
                var itemId = this.GetItemId(itemMatch.Item);

                // Add the reference to the item against the node for each of the word positions.
                var nodeId = persistedNode.InternalId;
                foreach (var position in itemMatch.Positions)
                {
                    this.index.PersistedEntryManager.AddNodeItemEntry(nodeId, itemId, position);
                }
            }
        }
    }
}

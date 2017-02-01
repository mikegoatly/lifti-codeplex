// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System;
    using System.Collections.Generic;

    using Lifti.Extensibility;

    /// <summary>
    /// The full text index implementation that adds the ability to remove and update items in the index.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to store in the index. This is the type that will
    /// be retrieved from the index.</typeparam>
    /// <example>
    /// <code><![CDATA[
    /// private UpdatableFullTextIndex<string> index;
    /// 
    /// void Main()
    /// {
    ///     this.index = new UpdatableFullTextIndex<string>(filename => GetDocumentText(filename))
    ///     {
    ///         WordSplitter = new StemmingWordSplitter(),
    ///         QueryParser = new LiftiQueryParser()
    ///     };
    /// 
    ///     this.index.Index(Directory.GetFiles(@"c:\MyDocumentStore"));
    /// 
    ///     Console.WriteLine("Removing documents containing the words 'design' and 'document'");
    ///     var fileNames = this.index.Search("design & document").ToArray();
    ///     this.index.Remove(fileNames);
    /// }
    /// 
    /// private string GetDocumentText(string filename)
    /// {
    ///     return File.ReadAllText(filename);
    /// }
    /// ]]></code>
    /// </example>

    public class UpdatableFullTextIndex<TKey> : FullTextIndex<TKey>, IUpdatableFullTextIndex<TKey>
    {
        /// <summary>
        /// The lookup of all the nodes indexed against the keys contained in the index.
        /// </summary>
        private readonly Dictionary<TKey, HashSet<IndexNode<TKey>>> keyNodes = new Dictionary<TKey, HashSet<IndexNode<TKey>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatableFullTextIndex&lt;TKey&gt;"/> class.
        /// </summary>
        public UpdatableFullTextIndex()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatableFullTextIndex&lt;TKey&gt;"/> class.
        /// </summary>
        /// <param name="extensibilityService">The extensibility service.</param>
        protected UpdatableFullTextIndex(IIndexExtensibilityService<TKey> extensibilityService)
            : base(extensibilityService)
        {
        }

        /// <summary>
        /// Gets the indexed items.
        /// </summary>
        /// <value>The indexed items.</value>
        internal IEnumerable<TKey> IndexedItems
        {
            get { return this.keyNodes.Keys; }
        }

        /// <summary>
        /// Removes the specified item key from the index.
        /// </summary>
        /// <param name="itemKey">The item to remove from the index.</param>
        public virtual void Remove(TKey itemKey)
        {
            using (this.LockManager.AcquireWriteLock())
            {
                this.RemoveItem(itemKey);
            }
        }

        /// <summary>
        /// Removes the specified item keys from the index.
        /// </summary>
        /// <param name="itemKeys">The item keys to remove from the index.</param>
        public virtual void Remove(IEnumerable<TKey> itemKeys)
        {
            if (itemKeys == null)
            {
                throw new ArgumentNullException(nameof(itemKeys));
            }

            using (this.LockManager.AcquireWriteLock())
            {
                foreach (var item in itemKeys)
                {
                    this.RemoveItem(item);
                }
            }
        }

        /// <summary>
        /// Determines whether the index currently has the specified item key in its index.
        /// </summary>
        /// <param name="itemKey">The item key to check for.</param>
        /// <returns>
        ///   <c>true</c> if the index contains the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TKey itemKey)
        {
            using (this.LockManager.AcquireReadLock())
            {
                return this.ContainsKey(itemKey);
            }
        }

        /// <summary>
        /// Called when a word is indexed against an item.
        /// </summary>
        /// <param name="item">The item the word was indexed against.</param>
        /// <param name="node">The node at the end of the indexed word.</param>
        protected override void OnItemWordIndexed(TKey item, IndexNode<TKey> node)
        {
            HashSet<IndexNode<TKey>> nodeList;
            if (!this.keyNodes.TryGetValue(item, out nodeList))
            {
                nodeList = new HashSet<IndexNode<TKey>>();
                this.keyNodes.Add(item, nodeList);
            }

            nodeList.Add(node);
        }

        /// <summary>
        /// Can be called when an item has been removed from a node. This might be because the node has been invalidated,
        /// in the case of a persisted index.
        /// </summary>
        /// <param name="item">The item that has been removed from a node.</param>
        /// <param name="node">The node the item was removed from.</param>
        protected void OnItemWordRemoved(TKey item, IndexNode<TKey> node)
        {
            HashSet<IndexNode<TKey>> nodeList;
            if (this.keyNodes.TryGetValue(item, out nodeList))
            {
                nodeList.Remove(node);
            }
        }

        /// <summary>
        /// Indexes the item key against the given text.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemText">The item text.</param>
        protected override void IndexItem(TKey itemKey, IEnumerable<string> itemText)
        {
            // First make sure the item is removed from the index, if it
            // is already indexed
            if (this.ContainsKey(itemKey))
            {
                this.RemoveItem(itemKey);
            }

            base.IndexItem(itemKey, itemText);
        }

        /// <summary>
        /// Determines whether the specified key exists in the index.
        /// </summary>
        /// <param name="item">The item key to check for.</param>
        /// <returns>
        ///   <c>true</c> if the index contains the specified item key; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool ContainsKey(TKey item)
        {
            return this.keyNodes.ContainsKey(item);
        }

        /// <summary>
        /// Determines the number of items currently held in the index.
        /// </summary>
        /// <returns>The number of items in the index.</returns>
        protected override int DetermineItemCount()
        {
            return this.keyNodes.Count;
        }

        /// <summary>
        /// Removes the given item from the index.
        /// </summary>
        /// <param name="item">The item to remove from the index.</param>
        private void RemoveItem(TKey item)
        {
            if (!this.ContainsKey(item))
            {
                return;
            }

            this.Extensibility.OnItemRemovalStarted(item);

            HashSet<IndexNode<TKey>> nodeList;
            if (this.keyNodes.TryGetValue(item, out nodeList))
            {
                foreach (var node in nodeList)
                {
                    node.DeindexItem(item);
                }

                this.keyNodes.Remove(item);
            }

            this.Extensibility.OnItemRemovalCompleted(item);
        }
    }
}

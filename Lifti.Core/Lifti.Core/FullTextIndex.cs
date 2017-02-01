// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System;
    using System.Collections.Generic;

    using Lifti.Extensibility;
    using Lifti.Locking;
    using Lifti.Querying;

    /// <summary>
    /// <para>The full text index class capable of indexing items against associated text.</para>
    /// <para>Note that <see cref="FullTextIndex{TKey}"/> only supports writing items once, i.e. you
    /// cannot remove or update an item once it has be indexed. If you need this functionality you should
    /// use the <see cref="UpdatableFullTextIndex{TKey}"/></para>
    /// </summary>
    /// <typeparam name="TKey">The type of the key to store in the index. This is the type that will
    /// be retrieved from the index.</typeparam>
    /// <example>
    /// <code><![CDATA[
    /// private FullTextIndex<string> index;
    /// 
    /// void Main()
    /// {
    ///     this.index = new FullTextIndex<string>()
    ///     {
    ///         WordSplitter = new StemmingWordSplitter(),
    ///         QueryParser = new LiftiQueryParser()
    ///     };
    /// 
    ///     this.index.Index(Directory.GetFiles(@"c:\MyDocumentStore"), f => GetDocumentText(f));
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
    public class FullTextIndex<TKey> : IFullTextIndex<TKey>, IDisposable
    {
        /// <summary>
        /// The number of indexed items held.
        /// </summary>
        private int count;

        /// <summary>
        /// The lock manager for this instance.
        /// </summary>
        private ILockManager lockManager;

        /// <summary>
        /// The word splitter implementation used when searching for words.
        /// </summary>
        private IWordSplitter searchWordSplitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FullTextIndex&lt;TKey&gt;"/> class.
        /// </summary>
        public FullTextIndex()
            : this(new IndexExtensibilityService<TKey>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullTextIndex&lt;TKey&gt;"/> class.
        /// </summary>
        /// <param name="extensibilityService">The extensibility service.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive - this instance lasts the lifetime of the class")]
        protected FullTextIndex(IIndexExtensibilityService<TKey> extensibilityService)
        {
            this.Extensibility = extensibilityService;

            this.LockManager = new LockManager();
            this.WordSplitter = new WordSplitter();
            this.QueryParser = new SimpleQueryParser();

            this.RootNode = new IndexNode<TKey>(this, '\0');
        }

        /// <inheritdoc />
        public ILockManager LockManager
        {
            get
            {
                return this.lockManager;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.lockManager = value;
            }
        }

        /// <inheritdoc />
        public IIndexExtensibilityService<TKey> Extensibility { get; }

        /// <inheritdoc />
        public virtual int Count
        {
            get
            {
                using (this.LockManager.AcquireReadLock())
                {
                    return this.DetermineItemCount();
                }
            }
        }

        /// <inheritdoc />
        public IWordSplitter WordSplitter
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IWordSplitter SearchWordSplitter
        {
            get
            {
                return this.searchWordSplitter ?? this.WordSplitter;
            }

            set
            {
                this.searchWordSplitter = value;
            }
        }

        /// <inheritdoc />
        public IQueryParser QueryParser
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IndexNode<TKey> RootNode
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public virtual void Index<TItem>(IEnumerable<TItem> items, Func<TItem, TKey> readKey, Func<TItem, string> readText)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (readKey == null)
            {
                throw new ArgumentNullException(nameof(readKey));
            }

            if (readText == null)
            {
                throw new ArgumentNullException(nameof(readText));
            }

            using (this.LockManager.AcquireWriteLock())
            {
                foreach (var item in items)
                {
                    this.IndexItem(readKey(item), new[] { readText(item) });
                }
            }
        }

        /// <inheritdoc />
        public virtual void Index<TItem>(TItem item, Func<TItem, TKey> readKey, Func<TItem, string> readText)
        {
            if (readKey == null)
            {
                throw new ArgumentNullException(nameof(readKey));
            }

            if (readText == null)
            {
                throw new ArgumentNullException(nameof(readText));
            }

            using (this.LockManager.AcquireWriteLock())
            {
                this.IndexItem(readKey(item), new[] { readText(item) });
            }
        }

        /// <inheritdoc />
        public virtual void Index(IEnumerable<TKey> itemKeys, Func<TKey, string> indexText)
        {
            if (itemKeys == null)
            {
                throw new ArgumentNullException(nameof(itemKeys));
            }

            if (indexText == null)
            {
                throw new ArgumentNullException(nameof(indexText));
            }

            using (this.LockManager.AcquireWriteLock())
            {
                foreach (var itemKey in itemKeys)
                {
                    this.IndexItem(itemKey, new[] { indexText(itemKey) });
                }
            }
        }

        /// <inheritdoc />
        public virtual void Index(TKey itemKey, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            using (this.LockManager.AcquireWriteLock())
            {
                this.IndexItem(itemKey, new[] { text });
            }
        }

        /// <inheritdoc />
        public virtual void Index(TKey itemKey, IEnumerable<string> text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            using (this.LockManager.AcquireWriteLock())
            {
                this.IndexItem(itemKey, text);
            }
        }

        /// <inheritdoc />
        public IEnumerable<TKey> Search(string query)
        {
            // Parse the query using the current query parser implementation
            var parsedQuery = this.QueryParser.ParseQuery(query, this.SearchWordSplitter);
            return this.Search(parsedQuery);
        }

        /// <inheritdoc />
        public IEnumerable<TKey> Search(IFullTextQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (this.LockManager.AcquireReadLock())
            {
                var context = new QueryContext<TKey>(this.RootNode);
                return query.Execute(context);
            }
        }

        /// <inheritdoc />
        public virtual IndexNode<TKey> CreateIndexNode(char character)
        {
            return new IndexNode<TKey>(this, character);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines the number of items currently held in the index.
        /// </summary>
        /// <returns>The number of items in the index.</returns>
        protected virtual int DetermineItemCount()
        {
            return this.count;
        }

        /// <summary>
        /// Called when a word is indexed against an item.
        /// </summary>
        /// <param name="item">The item the word was indexed against.</param>
        /// <param name="node">The node at the end of the indexed word.</param>
        protected virtual void OnItemWordIndexed(TKey item, IndexNode<TKey> node)
        {
        }

        /// <summary>
        /// Indexes the item key against the given text.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemText">The item text.</param>
        protected virtual void IndexItem(TKey itemKey, IEnumerable<string> itemText)
        {
            this.Extensibility.OnItemIndexingStarted(itemKey);

            foreach (var word in this.WordSplitter.SplitWords(itemText))
            {
                this.OnItemWordIndexed(itemKey, this.RootNode.IndexItem(itemKey, word.Word, word.GetLocations()));
            }

            this.count++;

            this.Extensibility.OnItemIndexingCompleted(itemKey);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.LockManager.Dispose();
            }
        }
    }
}

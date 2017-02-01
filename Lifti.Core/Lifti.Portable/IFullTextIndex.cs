// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    #region Using statements

    using System;
    using System.Collections.Generic;

    using Lifti.Extensibility;
    using Lifti.Locking;
    using Lifti.Querying;

    #endregion

    /// <summary>
    /// The interface implemented by full text indexes.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to store in the index. This is the type that will
    /// be retrieved from the index.</typeparam>
    public interface IFullTextIndex<TKey>
    {
        /// <summary>
        /// Gets the <see cref="IIndexExtensibilityService{TKey}"/> for the index. 
        /// Add-ins can be registered against this service and hook into various events 
        /// raised throughout the lifetime of the index.
        /// </summary>
        /// <value>
        /// The extensibility service for the index.
        /// </value>
        IIndexExtensibilityService<TKey> Extensibility { get; }

        /// <summary>
        /// Gets or sets the word splitter capable of determining the unique words in a 
        /// piece of text. This is used for splitting the words from text to index an item
        /// with. It is also used to split the text being used to search the index with, if the
        /// <see cref="IFullTextIndex&lt;TKey&gt;.SearchWordSplitter"/> property is null.
        /// </summary>
        /// <value>The word splitter instance used by the index to split indexed text.</value>
        IWordSplitter WordSplitter { get; set; }

        /// <summary>
        /// Gets or sets the word splitter capable of determining the unique words in a 
        /// piece of text. This is used solely for splitting the words from text being used 
        /// to search the index with. If this is not set then the word splitter specified
        /// in the <see cref="IFullTextIndex&lt;TKey&gt;.WordSplitter"/> property will be used
        /// for this purpose.
        /// </summary>
        /// <remarks>This only needs to be specified if the behavior of the word splitter
        /// needs to differ when indexing and searching text. For example, you might be
        /// using an <see cref="XmlWordSplitter"/> to index text, and just need a
        /// <see cref="StemmingWordSplitter"/> when searching on text.</remarks>
        /// <value>The word splitter instance used by the index to split search text.</value>
        IWordSplitter SearchWordSplitter { get; set; }

        /// <summary>
        /// Gets or sets the query parser used by this instance to parse a textual query into
        /// an <see cref="IFullTextQuery"/> instance.
        /// </summary>
        /// <value>The query parser instance used by the index to parse search queries.</value>
        IQueryParser QueryParser { get; set; }

        /// <summary>
        /// Gets the number of indexed items.
        /// </summary>
        /// <value>The number of indexed items.</value>
        int Count { get; }

        /// <summary>
        /// Gets the root node of the index.
        /// </summary>
        /// <value>The root node.</value>
        IndexNode<TKey> RootNode { get; }

        /// <summary>
        /// Gets or sets the lock manager for this full text index.
        /// </summary>
        /// <remarks>
        /// <para>When enabled, this manages concurrent access so that multiple threads can safely access the index.</para>
        /// <para>By default the lock manager is enabled - if you need to disable it, set the <see cref="ILockManager.Enabled"/> property.</para>
        /// </remarks>
        /// <value>The lock manager to use.</value>
        ILockManager LockManager { get; set; }

        /// <summary>
        /// Indexes a set of arbitrary items in the index.
        /// </summary>
        /// <remarks>For each item in <paramref name="items"/> the <paramref name="readKey"/> delegate will be
        /// used to determine the key to store in the index, and the <paramref name="readText"/> delegate
        /// will be used to read the text to index the key against.</remarks>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="items">The items to index.</param>
        /// <param name="readKey">A delegate capable of reading the TKey from the item.</param>
        /// <param name="readText">A delegate capable of reading the text from the item.</param>
        void Index<TItem>(IEnumerable<TItem> items, Func<TItem, TKey> readKey, Func<TItem, string> readText);

        /// <summary>
        /// Indexes an arbitrary class in the index.
        /// </summary>
        /// <remarks>The <paramref name="readKey"/> delegate will be used to determine the key to store in the index, 
        /// and the <paramref name="readText"/> delegate will be used to read the text to index the key against.</remarks>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="item">The item to index.</param>
        /// <param name="readKey">A delegate capable of reading the TKey from the item.</param>
        /// <param name="readText">A delegate capable of reading the text from the item.</param>
        void Index<TItem>(TItem item, Func<TItem, TKey> readKey, Func<TItem, string> readText);

        /// <summary>
        /// Indexes a set of item keys in the index.
        /// </summary>
        /// <remarks>The provided delegate will be used to determine the relevant text to index 
        /// each key against.</remarks>
        /// <param name="itemKeys">The item keys to index.</param>
        /// <param name="indexText">A delegate capable of obtaining the text to index for a key.</param>
        void Index(IEnumerable<TKey> itemKeys, Func<TKey, string> indexText);

        /// <summary>
        /// Indexes an item key in the index.
        /// </summary>
        /// <param name="itemKey">The item key to index.</param>
        /// <param name="text">The text to index the item against.</param>
        void Index(TKey itemKey, string text);

        /// <summary>
        /// Indexes an item key in the index.
        /// </summary>
        /// <param name="itemKey">The item key to index.</param>
        /// <param name="text">The text to index the item against.</param>
        void Index(TKey itemKey, IEnumerable<string> text);

        /// <summary>
        /// Searches the index with the specified search text - only items that contain words
        /// that match exactly or start with all the search words will be returned.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>The matching items.</returns>
        IEnumerable<TKey> Search(string query);

        /// <summary>
        /// Searches the index using the specified query.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>The matching items.</returns>
        IEnumerable<TKey> Search(IFullTextQuery query);

        /// <summary>
        /// Creates a new node that will be contained in the index.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>The created node.</returns>
        /// <remarks><para>The creation of new node instance is generally managed by the LIFTI framework, and you
        /// probably won't need to use this method.</para>
        /// <para>This method can be overridden by deriving classes to allow for a different derivative 
        /// of <see cref="IndexNode{TKey}"/> to be stored in the index.</para></remarks>
        IndexNode<TKey> CreateIndexNode(char character);
    }
}

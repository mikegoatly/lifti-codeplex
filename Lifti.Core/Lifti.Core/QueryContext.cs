// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The context specific to a currently executing query. Instances of <see cref="QueryContext{TKey}"/> are automatically created
    /// by the <see cref="FullTextIndex{TKey}"/> class when a query is executed.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
        Justification = "Comparing for equality never happens and wouldn't really make sense anyway")]
    public struct QueryContext<TKey> : IQueryContext<TKey>
    {
        /// <summary>
        /// The root node of the current query context.
        /// </summary>
        private readonly IndexNode<TKey> rootNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext{TKey}"/> struct.
        /// </summary>
        /// <param name="rootNode">The root node of the full text index to search in.</param>
        internal QueryContext(IndexNode<TKey> rootNode)
        {
            this.rootNode = rootNode;
        }

        /// <summary>
        /// Matches the given search word in the index, returning the items that contain or start with it.
        /// </summary>
        /// <param name="searchWord">The search word.</param>
        /// <returns>The matched items.</returns>
        public IEnumerable<ItemWordMatch<TKey>> MatchWord(string searchWord)
        {
            var endNode = this.MatchNode(searchWord);
            return endNode == null ? Enumerable.Empty<ItemWordMatch<TKey>>() : endNode.GetDirectAndChildItems();
        }

        /// <summary>
        /// Matches the given search word in the index, returning the items that match it exactly.
        /// </summary>
        /// <param name="searchWord">The search word.</param>
        /// <returns>The matched items.</returns>
        public IEnumerable<ItemWordMatch<TKey>> MatchWordExact(string searchWord)
        {
            var endNode = this.MatchNode(searchWord);
            return endNode == null ? Enumerable.Empty<ItemWordMatch<TKey>>() : endNode.GetDirectItems();
        }

        /// <summary>
        /// Matches the full text node at the end of the given search word.
        /// </summary>
        /// <param name="searchWord">The search word.</param>
        /// <returns>The node that represents the end of the search word, or null if the word could
        /// not be matched in the index.</returns>
        private IndexNode<TKey> MatchNode(string searchWord)
        {
            var currentNode = this.rootNode;

            foreach (var letter in searchWord)
            {
                currentNode = currentNode.Match(letter);

                if (currentNode == null)
                {
                    // This search word matches no items
                    break;
                }
            }

            return currentNode;
        }
    }
}
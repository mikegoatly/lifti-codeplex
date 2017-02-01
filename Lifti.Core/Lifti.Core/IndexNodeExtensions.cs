// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    /// <summary>
    /// Extension methods for the <see cref="IndexNode{TKey}" /> class.
    /// </summary>
    public static class IndexNodeExtensions
    {
        /// <summary>
        /// Determines whether the node is the root node of an index.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of item in the index.
        /// </typeparam>
        /// <param name="node">
        /// The node to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the node is the root node, otherwise <c>false</c>.
        /// </returns>
        public static bool IsRootNode<TKey>(this IndexNode<TKey> node)
        {
            return node.IndexedCharacter == '\0';
        }
    }
}
// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Extension methods for implementations of <see cref="IFullTextIndex{Key}"/>.
    /// </summary>
    public static class IFullTextIndexExtensions
    {
        /// <summary>
        /// Enumerates all the indexed words in the given index.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of item in the index.
        /// </typeparam>
        /// <param name="index">
        /// The index to enumerate the words in.
        /// </param>
        /// <returns>
        /// An enumerable capable of iterating through all the words contained in the index.
        /// </returns>
        public static IEnumerable<string> EnumerateIndexedWords<TKey>(this IFullTextIndex<TKey> index)
        {
            using (index.LockManager.AcquireReadLock())
            {
                return EnumerateWordsFromNode(new StringBuilder(), index.RootNode);
            }
        }

        /// <summary>
        /// Enumerates all the indexed words that start with the given value.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of item in the index.
        /// </typeparam>
        /// <param name="index">
        /// The index to enumerate the words in.
        /// </param>
        /// <param name="startsWith">
        /// The text to start searching with. Only words that start with this value will be returned.
        /// </param>
        /// <returns>
        /// An enumerable capable of iterating through all matching words contained in the index.
        /// </returns>
        public static IEnumerable<string> EnumerateIndexedWords<TKey>(this IFullTextIndex<TKey> index, string startsWith)
        {
            using (index.LockManager.AcquireReadLock())
            {
                // Match all the characters in the text to start searching from
                var builder = new StringBuilder();
                var node = index.RootNode;

                if (!string.IsNullOrEmpty(startsWith))
                {
                    foreach (var c in startsWith)
                    {
                        node = node.Match(c);
                        if (node == null)
                        {
                            break;
                        }

                        builder.Append(c);
                    }
                }

                // Don't do anything if the "starts with" text couldn't be matched to a node.
                if (node != null)
                {
                    // This function pushes the start node's character onto the string builder
                    // which will already have happened above, so remove it
                    if (builder.Length > 0)
                    {
                        builder.Length -= 1;
                    }

                    return EnumerateWordsFromNode(builder, node);
                }

                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Enumerates all the indexed words that start with the given node.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of item in the index.
        /// </typeparam>
        /// <param name="builder">
        /// The string builder used to build the returned words. On entry to this method it will
        /// contain the text that preceeds the character contained in <paramref name="node"/>.
        /// </param>
        /// <param name="node">
        /// The next node to process.
        /// </param>
        /// <returns>
        /// An enumerable capable of iterating through all matching words contained under the given node.
        /// </returns>
        private static IEnumerable<string> EnumerateWordsFromNode<TKey>(StringBuilder builder, IndexNode<TKey> node)
        {
            if (!node.IsRootNode())
            {
                // Add this node to the string builder
                builder.Append(node.IndexedCharacter);

                if (node.ContainsDirectItems)
                {
                    // This node contains items, so return the currently built word
                    yield return builder.ToString();
                }
            }

            // Recurse down the child nodes and yield any words returned therein.
            foreach (var n in node.GetChildNodes())
            {
                foreach (var childWord in EnumerateWordsFromNode(builder, n))
                {
                    yield return childWord;
                }
            }

            if (!node.IsRootNode())
            {
                // Pop this character off of the string builder
                builder.Length -= 1;
            }
        }
    }
}
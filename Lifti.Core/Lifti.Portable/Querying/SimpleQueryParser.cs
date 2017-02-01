// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    #region Using statements

    using System;

    #endregion

    /// <summary>
    /// A very basic implementation of a query parser. All words in the query text are parsed as LIKE operators,
    /// connected with AND operators. This simulates the simplistic search patterns that the original
    /// LIFTI code used.
    /// </summary>

    public class SimpleQueryParser : IQueryParser
    {
        /// <summary>
        /// Parses the specified query text, returning a representative instance of <see cref="IFullTextQuery"/>.
        /// </summary>
        /// <param name="queryText">The query text to parse.</param>
        /// <param name="wordSplitter">The word splitter implementation used to split out individual words
        /// from composite statements.</param>
        /// <returns>The parsed query.</returns>
        public IFullTextQuery ParseQuery(string queryText, IWordSplitter wordSplitter)
        {
            if (wordSplitter == null)
            {
                throw new ArgumentNullException(nameof(wordSplitter));
            }

            IQueryPart root = null;
            foreach (var word in wordSplitter.SplitWords(queryText))
            {
                // Use a like operator to match the word
                IQueryPart likePart = new LikeWordQueryPart(word.Word);

                // If there have already been some query parts processed, combine them with an AND operator.
                root = root == null ? likePart : new AndQueryOperator(root, likePart);
            }

            return new FullTextQuery(root);
        }
    }
}

// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A full text query part that specifies that all the contained words 
    /// must appear next to each other, in order.
    /// </summary>
    public class AdjacentWordsQueryPart : IQueryPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdjacentWordsQueryPart"/> class.
        /// </summary>
        /// <param name="words">The words that must appear in order.</param>
        public AdjacentWordsQueryPart(IEnumerable<IWordQueryPart> words)
        {
            this.Words = words;
        }

        /// <summary>
        /// Gets the words that must appear adjacent to each other.
        /// </summary>
        /// <value>The <see cref="IEnumerable&lt;IWordQueryPart&gt;"/> of words that must appear in sequence.</value>
        public IEnumerable<IWordQueryPart> Words
        {
            get; }

        /// <summary>
        /// Executes this query part instance against the specified query context.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="context">The query context to execute the query against.</param>
        /// <returns>
        /// The query result that contains the matched items.
        /// </returns>
        public IQueryResult<TItem> ExecuteQuery<TItem>(IQueryContext<TItem> context)
        {
            IEnumerable<ItemWordMatch<TItem>> currentMatches = null;
            foreach (var word in this.Words)
            {
                var results = word.ExecuteQuery(context);
                if (currentMatches == null)
                {
                    currentMatches = results.Matches;
                }
                else
                {
                    currentMatches = from cm in currentMatches
                                     join m in results.Matches on cm.Item equals m.Item
                                     let match = cm.PositionalIntersect(m, 0, 1)
                                     where match.Success
                                     select match;
                }
            }

            return new QueryResult<TItem>(currentMatches);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Words.Aggregate((string)null, (current, word) => (current == null ? "\"" : current + " ") + word.ToString()) + "\"";
        }
    }
}

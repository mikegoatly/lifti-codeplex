// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// A query part that matches on a single word in some way.
    /// </summary>
    public abstract class WordQueryPart : IWordQueryPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WordQueryPart"/> class.
        /// </summary>
        /// <param name="word">The word to match on.</param>
        protected WordQueryPart(string word)
        {
            this.Word = word;
        }

        /// <summary>
        /// Gets or sets the word to match on.
        /// </summary>
        /// <value>The word to match on.</value>
        public string Word
        {
            get;
            set;
        }

        /// <summary>
        /// Executes this query part instance against the specified query context.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="context">The query context to execute the query against.</param>
        /// <returns>
        /// The query result that contains the matched items.
        /// </returns>
        public abstract IQueryResult<TItem> ExecuteQuery<TItem>(IQueryContext<TItem> context);
    }
}

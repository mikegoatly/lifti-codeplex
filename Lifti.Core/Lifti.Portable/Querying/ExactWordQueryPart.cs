// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    #region Using statements

    using System;

    #endregion

    /// <summary>
    /// A query part that matches items that are indexed against a single word.
    /// </summary>
    public class ExactWordQueryPart : WordQueryPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExactWordQueryPart"/> class.
        /// </summary>
        /// <param name="word">The word to match on.</param>
        public ExactWordQueryPart(string word)
            : base(word)
        {
        }

        /// <summary>
        /// Executes this query part instance against the specified query context.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="context">The query context to execute the query against.</param>
        /// <returns>The query result that contains the matched items.</returns>
        public override IQueryResult<TItem> ExecuteQuery<TItem>(IQueryContext<TItem> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new QueryResult<TItem>(context.MatchWordExact(this.Word));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "EXACT(" + this.Word + ")";
        }
    }
}

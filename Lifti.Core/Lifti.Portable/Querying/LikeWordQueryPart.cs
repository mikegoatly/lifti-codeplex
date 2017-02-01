// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    #region Using statements

    using System;

    #endregion

    /// <summary>
    /// A query part that matches items that are indexed against words
    /// that start with, or match exactly, a specified word.
    /// </summary>
    public class LikeWordQueryPart : WordQueryPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LikeWordQueryPart"/> class.
        /// </summary>
        /// <param name="word">The word to match on.</param>
        public LikeWordQueryPart(string word)
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

            return new QueryResult<TItem>(context.MatchWord(this.Word));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "LIKE(" + this.Word + ")";
        }
    }
}

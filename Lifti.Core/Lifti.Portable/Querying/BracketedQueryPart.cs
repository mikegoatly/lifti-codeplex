// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// <see cref="BracketedQueryPart"/>s are used to ensure that any user-grouped statements are not messed with
    /// by any implied binary operator precedence.
    /// </summary>
    public class BracketedQueryPart : IQueryPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BracketedQueryPart"/> class.
        /// </summary>
        /// <param name="statement">The statement contained within the brackets.</param>
        public BracketedQueryPart(IQueryPart statement)
        {
            this.Statement = statement;
        }

        /// <summary>
        /// Gets the bracketed statement contained within the user specified brackets.
        /// </summary>
        /// <value>The <see cref="IQueryPart"/> statement within the brackets.</value>
        public IQueryPart Statement
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
            // Just defer execution to the contained statement.
            return this.Statement.ExecuteQuery(context);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "[" + this.Statement + "]";
        }
    }
}

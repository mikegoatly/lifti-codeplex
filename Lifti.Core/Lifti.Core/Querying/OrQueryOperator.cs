// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// An OR binary operator that performs a union (distinct) of two results sets.
    /// </summary>
    public class OrQueryOperator : BinaryQueryOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrQueryOperator"/> class.
        /// </summary>
        /// <param name="left">The left part of the query operator.</param>
        /// <param name="right">The right part of the query operator.</param>
        public OrQueryOperator(IQueryPart left, IQueryPart right)
            : base(left, right)
        {
        }

        /// <summary>
        /// Gets the precedence of this operator. This value dictates how binary operators will be combined.
        /// </summary>
        /// <value>The operator precedence.</value>
        public override OperatorPrecedence Precedence => OperatorPrecedence.Or;

        /// <summary>
        /// Executes this query part instance against the specified query context.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="context">The query context to execute the query against.</param>
        /// <returns>
        /// The query result that contains the matched items.
        /// </returns>
        public override IQueryResult<TItem> ExecuteQuery<TItem>(IQueryContext<TItem> context)
        {
            return this.Left.ExecuteQuery(context).Union(this.Right.ExecuteQuery(context));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "(" + this.Left + " OR " + this.Right + ")";
        }
    }
}

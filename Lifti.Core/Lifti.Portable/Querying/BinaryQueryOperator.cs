// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// An abstract class representing a binary query operator, e.g. an AND or an OR operator.
    /// </summary>
    public abstract class BinaryQueryOperator : IBinaryQueryOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryQueryOperator"/> class.
        /// </summary>
        /// <param name="left">The left part of the query operator.</param>
        /// <param name="right">The right part of the query operator.</param>
        protected BinaryQueryOperator(IQueryPart left, IQueryPart right)
        {
            this.Left = left;
            this.Right = right;
        }

        /// <summary>
        /// Gets or sets the left part of the query operator.
        /// </summary>
        /// <value>The left query part.</value>
        public IQueryPart Left
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the right part of the query operator.
        /// </summary>
        /// <value>The right query part.</value>
        public IQueryPart Right
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the precedence of this operator. This value dictates how binary operators will be combined.
        /// </summary>
        /// <value>The operator precedence.</value>
        public abstract OperatorPrecedence Precedence
        {
            get;
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

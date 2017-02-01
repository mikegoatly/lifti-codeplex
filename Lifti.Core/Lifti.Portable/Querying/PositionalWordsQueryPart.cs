// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    #region Using statements

    using System.Linq;

    #endregion

    /// <summary>
    /// A full text query part that specifies that the left and right words in the query part
    /// must be combined with respect to some locational predicate.
    /// </summary>
    public abstract class PositionalWordsQueryPart : BinaryQueryOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PositionalWordsQueryPart"/> class.
        /// </summary>
        /// <param name="left">The left part of the query operator.</param>
        /// <param name="right">The right part of the query operator.</param>
        /// <param name="leftTolerance">The left positional tolerance.</param>
        /// <param name="rightTolerance">The right positional tolerance.</param>
        protected PositionalWordsQueryPart(IQueryPart left, IQueryPart right, int leftTolerance, int rightTolerance)
            : base(left, right)
        {
            this.LeftTolerance = leftTolerance;
            this.RightTolerance = rightTolerance;
        }

        /// <summary>
        /// Gets the precedence of this operator. This value dictates how binary operators will be combined.
        /// </summary>
        /// <value>The operator precedence.</value>
        public override OperatorPrecedence Precedence
        {
            get { return OperatorPrecedence.Locational; }
        }

        /// <summary>
        /// Gets or sets the left tolerance.
        /// </summary>
        /// <value>The left tolerance.</value>
        private int LeftTolerance
        {
            get; }

        /// <summary>
        /// Gets or sets the right tolerance.
        /// </summary>
        /// <value>The right tolerance.</value>
        private int RightTolerance
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
        public override IQueryResult<TItem> ExecuteQuery<TItem>(IQueryContext<TItem> context)
        {
            return new QueryResult<TItem>(from m in this.Left.ExecuteQuery(context).Matches
                                          join cm in this.Right.ExecuteQuery(context).Matches on m.Item equals cm.Item
                                          let match = m.PositionalIntersect(cm, this.LeftTolerance, this.RightTolerance)
                                          where match.Success
                                          select match);
        }
    }
}

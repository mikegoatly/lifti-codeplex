// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// A PRECEDING NEAR binary operator that performs an intersection of two results sets based on locality of the results.
    /// </summary>
    public class PrecedingNearQueryOperator : PositionalWordsQueryPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrecedingNearQueryOperator"/> class.
        /// </summary>
        /// <param name="left">The left part of the query operator.</param>
        /// <param name="right">The right part of the query operator.</param>
        public PrecedingNearQueryOperator(IQueryPart left, IQueryPart right)
            : this(left, right, 5)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrecedingNearQueryOperator"/> class.
        /// </summary>
        /// <param name="left">The left part of the query operator.</param>
        /// <param name="right">The right part of the query operator.</param>
        /// <param name="tolerance">The tolerance to consider when checking to see if
        /// words from the left and right expression match.</param>
        public PrecedingNearQueryOperator(IQueryPart left, IQueryPart right, int tolerance)
            : base(left, right, 0, tolerance)
        {   
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "(" + this.Left + " PRECEDING NEAR " + this.Right + ")";
        }
    }
}

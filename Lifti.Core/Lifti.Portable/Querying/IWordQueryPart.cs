// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// The interface implemented by parts of a full text query that are explicitly searching
    /// for a word.
    /// </summary>
    public interface IWordQueryPart : IQueryPart
    {
        /// <summary>
        /// Gets or sets the word to match on.
        /// </summary>
        /// <value>The word to match on.</value>
        string Word { get; set; }
    }
}
// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// The interface implemented by classes capable of breaking 
    /// </summary>
    public interface IQueryParser
    {
        /// <summary>
        /// Parses the specified query text, returning a representative instance of <see cref="IFullTextQuery"/>.
        /// </summary>
        /// <param name="queryText">The query text to parse.</param>
        /// <param name="wordSplitter">The word splitter implementation used to split out individual words 
        /// from composite statements.</param>
        /// <returns>The parsed query.</returns>
        IFullTextQuery ParseQuery(string queryText, IWordSplitter wordSplitter);
    }
}

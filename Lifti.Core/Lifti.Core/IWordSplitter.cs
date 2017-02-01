// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System.Collections.Generic;

    /// <summary>
    /// The interface implemented by word splitter classes.
    /// </summary>
    public interface IWordSplitter
    {
        /// <summary>
        /// Splits the individual words out of the given text, ensuring that each word only appears once.
        /// </summary>
        /// <param name="text">The text to process.</param>
        /// <returns>The set of distinct words in the text in uppercase.</returns>
        IEnumerable<SplitWord> SplitWords(string text);

        /// <summary>
        /// Splits the individual words out of the given text, ensuring that each word only appears once.
        /// </summary>
        /// <param name="textFragments">The text to process.</param>
        /// <returns>The set of distinct words in the text in uppercase.</returns>
        IEnumerable<SplitWord> SplitWords(IEnumerable<string> textFragments);

        /// <summary>
        /// Normalizes the given word to a consistent format.
        /// </summary>
        /// <param name="word">
        /// The word to normalize.
        /// </param>
        /// <returns>
        /// The normalized word.
        /// </returns>
        string Normalize(string word);
    }
}

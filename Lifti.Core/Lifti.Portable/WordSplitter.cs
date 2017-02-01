// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    #region Using statements

    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion

    /// <summary>
    /// The default implementation of the a word splitter.
    /// </summary>

    public class WordSplitter : IWordSplitter
    {
        /// <inheritdoc />
        public virtual IEnumerable<SplitWord> SplitWords(string text)
        {
            return this.SplitWords(new[] { text });
        }

        /// <inheritdoc />
        public virtual IEnumerable<SplitWord> SplitWords(IEnumerable<string> textFragments)
        {
            return from w in this.EnumerateWords(textFragments)
                   group w by w.Word.ToUpperInvariant() into wg
                   select new SplitWord(
                       wg.Key, 
                       wg.SelectMany(g => g.GetLocations()).OrderBy(i => i).ToArray());
        }

        /// <inheritdoc />
        public virtual string Normalize(string word)
        {
            return word.ToUpperInvariant();
        }

        /// <summary>
        /// Enumerates the words in the given text.
        /// </summary>
        /// <param name="textFragments">The text to enumerate the words in.</param>
        /// <returns>The words identified in the text, in the order they appear.</returns>
        protected virtual IEnumerable<SplitWord> EnumerateWords(IEnumerable<string> textFragments)
        {
            var currentWord = new StringBuilder();
            var index = 0;
            foreach (var text in textFragments)
            {
                currentWord.Length = 0;

                foreach (var character in text)
                {
                    if (char.IsLetterOrDigit(character))
                    {
                        // This is a character of a word, so add it to the current word
                        currentWord.Append(character);
                    }
                    else if (character != '\'' &&
                        (char.IsSymbol(character) ||
                        char.IsPunctuation(character) ||
                        char.IsWhiteSpace(character)))
                    {
                        if (currentWord.Length > 0)
                        {
                            // Characters have been processed in the current word
                            // Yield it, and start a new word
                            yield return new SplitWord(currentWord.ToString(), index++);
                            currentWord.Length = 0;
                        }
                    }
                }

                if (currentWord.Length > 0)
                {
                    // Characters have been processed in the current word - this
                    // is the last in the text, so ensure it is yielded
                    yield return new SplitWord(currentWord.ToString(), index++);
                }
            }
        }
    }
}

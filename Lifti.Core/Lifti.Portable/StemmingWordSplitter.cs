// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    #region Using statements

    using System.Collections.Generic;
    using System.Linq;

    using Lifti.PorterStemmer;

    #endregion

    /// <summary>
    /// A word splitter that uses a <see cref="PorterStemmer"/> instance to stem words found in the text 
    /// prior to returning them. An instance of a <see cref="StemmingWordSplitter"/> can be associated to a
    /// <see cref="FullTextIndex{TKey}"/> via the <see cref="FullTextIndex{TKey}.WordSplitter"/> property.
    /// </summary>

    public class StemmingWordSplitter : WordSplitter
    {
        /// <summary>
        /// The word stemmer to use on words that have been split from text.
        /// </summary>
        private readonly Stemmer stemmer = new Stemmer();

        /// <inheritdoc />
        public override IEnumerable<SplitWord> SplitWords(IEnumerable<string> textFragments)
        {
            return from w in this.EnumerateWords(textFragments)
                   group w by this.Normalize(this.stemmer.Stem(w.Word)) into wg
                   select new SplitWord(
                       wg.Key, 
                       wg.SelectMany(g => g.GetLocations()).OrderBy(i => i).ToArray());
        }
    }
}
// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    #region Using statements

    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// A word splitter capable of only processing words contained within XML elements, and not the element names themselves.
    /// </summary>
    public class XmlWordSplitter : WordSplitter
    {
        /// <summary>
        /// The word splitter to use when splitting words contained within text elements.
        /// </summary>
        private readonly IWordSplitter textWordSplitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlWordSplitter"/> class.
        /// </summary>
        /// <param name="textWordSplitter">The word splitter to use when splitting words contained within text elements.</param>
        public XmlWordSplitter(IWordSplitter textWordSplitter)
        {
            this.textWordSplitter = textWordSplitter;
        }

        /// <inhertdoc />
        public override string Normalize(string word)
        {
            // Defer to the underlying word splitter to normalize the text.
            return this.textWordSplitter.Normalize(word);
        }

        /// <inheritdoc />
        protected override IEnumerable<SplitWord> EnumerateWords(IEnumerable<string> textFragments)
        {
            return this.textWordSplitter.SplitWords(this.EnumerateXmlTextSections(textFragments));
        }

        /// <summary>
        /// Enumerates all the text fragments within all the given XML text.
        /// </summary>
        /// <param name="textFragments">The XML to extract the text from.</param>
        /// <returns>An enumerable of all the text content in the XML.</returns>
        private IEnumerable<string> EnumerateXmlTextSections(IEnumerable<string> textFragments)
        {
            foreach (var text in textFragments)
            {
                var length = text.Length;
                var mark = 0;
                for (var i = 0; i < length; i++)
                {
                    if (text[i] == '<')
                    {
                        if (mark != i)
                        {
                            yield return text.Substring(mark, i - mark);
                        }

                        do
                        {
                            i++;
                        }
                        while (i < length && text[i] != '>');

                        mark = i + 1;
                    }
                }
            }
        }
    }
}
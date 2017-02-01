// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    /// <summary>
    /// Information about a word that has been split from a larger body of text, including its positional information.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Instances of this struct are not compared")]
    public struct SplitWord
    {
        /// <summary>
        /// The split word.
        /// </summary>
        private readonly string word;

        /// <summary>
        /// The index location(s) the word appeared at in the text.
        /// </summary>
        private readonly int[] locations;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitWord"/> struct.
        /// </summary>
        /// <param name="word">The split word.</param>
        /// <param name="location">The location the word appeared at.</param>
        public SplitWord(string word, int location)
        {
            this.word = word;
            this.locations = new[] { location };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitWord"/> struct.
        /// </summary>
        /// <param name="word">The split word.</param>
        /// <param name="locations">The index location(s) the word appeared at in the text.</param>
        public SplitWord(string word, int[] locations)
        {
            this.word = word;
            this.locations = locations;
        }

        /// <summary>
        /// Gets the split word.
        /// </summary>
        /// <value>The split word.</value>
        public string Word
        {
            get { return this.word; }
        }

        /// <summary>
        /// Gets the index location(s) the word appeared at in the text..
        /// </summary>
        /// <returns>The word index locations.</returns>
        public int[] GetLocations()
        {
            return this.locations;
        }
    }
}

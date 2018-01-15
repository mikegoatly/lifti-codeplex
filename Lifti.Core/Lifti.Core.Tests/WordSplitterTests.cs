// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    /// Tests for the word splitter class.
    /// </summary>
    [TestFixture]
    public class WordSplitterTests
    {
        /// <summary>
        /// Tests that an the empty string yields no tokens.
        /// </summary>
        [Test]
        public void EmptyStringYieldsNoTokens()
        {
            var splitter = new WordSplitter();
            Assert.AreEqual(0, splitter.SplitWords(string.Empty).Count());
        }

        /// <summary>
        /// Tests that one word yields one token.
        /// </summary>
        [Test]
        public void OneWordYieldsOneToken()
        {
            var splitter = new WordSplitter();

            var results = splitter.SplitWords("Test").ToArray();
            Assert.AreEqual(1, results.Length);
            
            Assert.AreEqual("TEST", results[0].Word);
            AssertSplitWordLocations(results[0].GetLocations(), 0);
        }

        /// <summary>
        /// Tests that multiple words yields multiple token.
        /// </summary>
        [Test]
        public void MultipleWordsYieldMultipleTokens()
        {
            var splitter = new WordSplitter();

            var results = splitter.SplitWords("The quick brown fox").ToArray();
            Assert.AreEqual(4, results.Length);

            Assert.AreEqual("THE", results[0].Word);
            AssertSplitWordLocations(results[0].GetLocations(), 0);
            Assert.AreEqual("QUICK", results[1].Word);
            AssertSplitWordLocations(results[1].GetLocations(), 1);
            Assert.AreEqual("BROWN", results[2].Word);
            AssertSplitWordLocations(results[2].GetLocations(), 2);
            Assert.AreEqual("FOX", results[3].Word);
            AssertSplitWordLocations(results[3].GetLocations(), 3);
        }

        /// <summary>
        /// Tests that a duplicate word only yields one result.
        /// </summary>
        [Test]
        public void DuplicateWordsYieldMultipleTokens()
        {
            var splitter = new WordSplitter();

            var results = splitter.SplitWords("The quick brown fox jumps over the lazy dog").ToArray();
            Assert.AreEqual(8, results.Length);

            Assert.AreEqual("THE", results[0].Word);
            AssertSplitWordLocations(results[0].GetLocations(), 0, 6);
            Assert.AreEqual("QUICK", results[1].Word);
            AssertSplitWordLocations(results[1].GetLocations(), 1);
            Assert.AreEqual("BROWN", results[2].Word);
            AssertSplitWordLocations(results[2].GetLocations(), 2);
            Assert.AreEqual("FOX", results[3].Word);
            AssertSplitWordLocations(results[3].GetLocations(), 3);
            Assert.AreEqual("JUMPS", results[4].Word);
            AssertSplitWordLocations(results[4].GetLocations(), 4);
            Assert.AreEqual("OVER", results[5].Word);
            AssertSplitWordLocations(results[5].GetLocations(), 5);
            Assert.AreEqual("LAZY", results[6].Word);
            AssertSplitWordLocations(results[6].GetLocations(), 7);
            Assert.AreEqual("DOG", results[7].Word);
            AssertSplitWordLocations(results[7].GetLocations(), 8);
        }

        /// <summary>
        /// Asserts the split word locations.
        /// </summary>
        /// <param name="locations">The locations.</param>
        /// <param name="expectedLocations">The expected locations.</param>
        private static void AssertSplitWordLocations(int[] locations, params int[] expectedLocations)
        {
            Assert.IsTrue(expectedLocations.SequenceEqual(locations));
        }
    }
}

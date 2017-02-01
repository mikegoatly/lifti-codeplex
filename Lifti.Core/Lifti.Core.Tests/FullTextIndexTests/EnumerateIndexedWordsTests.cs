// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.FullTextIndexTests
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the IFullTextIndexExtensions.EnumerateWordsFromNode{TKey} methods.
    /// </summary>
    [TestClass]
    public class EnumerateIndexedWordsTests
    {
        /// <summary>
        /// An empty index should result in no words being enumerated.
        /// </summary>
        [TestMethod]
        public void ShouldReturnNoWordsForEmptyIndex()
        {
            var index = new FullTextIndex<string>();
            Assert.AreEqual(0, index.EnumerateIndexedWords().Count());
        }

        /// <summary>
        /// With only one word indexed, one word should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldReturnSingleWordIfOnlyOneWordIndexed()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "test");

            var words = index.EnumerateIndexedWords().ToArray();
            Assert.AreEqual(1, words.Length);
            Assert.AreEqual("TEST", words[0]);
        }

        /// <summary>
        /// If multiple items are indexed against one word, that word should still only be returned once.
        /// </summary>
        [TestMethod]
        public void ShouldReturnOneWordIfMultipleItemsIndexedAgainstSameWord()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "test");
            index.Index("test2", "test");

            var words = index.EnumerateIndexedWords().ToArray();
            Assert.AreEqual(1, words.Length);
            Assert.AreEqual("TEST", words[0]);
        }

        /// <summary>
        /// If multiple items are indexed against one word, that word should still only be returned once.
        /// </summary>
        [TestMethod]
        public void ShouldReturnAllIndexedWordsForAllItems()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "This is a test");
            index.Index("test2", "This is another test");
            index.Index("test3", "Last one");

            var words = index.EnumerateIndexedWords().OrderBy(w => w).ToArray();
            Assert.IsTrue((new[] { "A", "ANOTHER", "IS", "LAST", "ONE", "TEST", "THIS" }).SequenceEqual(words));
        }

        /// <summary>
        /// If an empty prefix is searched for, then all indexed words should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldReturnAllIndexedWordsForAnEmptyPrefixSearch()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "This is a test");
            index.Index("test2", "This is another test");
            index.Index("test3", "Last one");

            var words = index.EnumerateIndexedWords(string.Empty).OrderBy(w => w).ToArray();
            Assert.IsTrue((new[] { "A", "ANOTHER", "IS", "LAST", "ONE", "TEST", "THIS" }).SequenceEqual(words));
        }

        /// <summary>
        /// If a null prefix is searched for, then all indexed words should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldReturnAllIndexedWordsForANullPrefixSearch()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "This is a test");
            index.Index("test2", "This is another test");
            index.Index("test3", "Last one");

            var words = index.EnumerateIndexedWords(null).OrderBy(w => w).ToArray();
            Assert.IsTrue((new[] { "A", "ANOTHER", "IS", "LAST", "ONE", "TEST", "THIS" }).SequenceEqual(words));
        }

        /// <summary>
        /// If a prefix is supplied, then only words starting with it should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldOnlyReturnWordsStartingWithPrefix()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "This is a test");
            index.Index("test2", "This is another test");
            index.Index("test3", "Last one");

            var words = index.EnumerateIndexedWords("T").OrderBy(w => w).ToArray();
            Assert.IsTrue((new[] { "TEST", "THIS" }).SequenceEqual(words));
        }

        /// <summary>
        /// If a prefix is supplied, but no indexed words start with it, no words should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldReturnNoWordsIfPrefixNotMatched()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "This is a test");
            index.Index("test2", "This is another test");
            index.Index("test3", "Last one");

            var words = index.EnumerateIndexedWords("Z").OrderBy(w => w).ToArray();
            Assert.AreEqual(0, words.Length);
        }

        /// <summary>
        /// If a prefix is supplied and it exactly matches a word, that word and any child words
        /// should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldOnlyReturnWordsStartingWithPrefixIncludingExactMatches()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "tested test testing nope");

            var words = index.EnumerateIndexedWords("TEST").OrderBy(w => w).ToArray();
            Assert.IsTrue((new[] { "TEST", "TESTED", "TESTING" }).SequenceEqual(words));
        }
    }
}
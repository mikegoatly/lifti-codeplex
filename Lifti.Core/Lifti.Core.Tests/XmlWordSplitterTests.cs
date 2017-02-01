// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the <see cref="XmlWordSplitter"/> class.
    /// </summary>
    [TestClass]
    public class XmlWordSplitterTests
    {
        /// <summary>
        /// Tests that an the empty string yields no tokens.
        /// </summary>
        [TestMethod]
        public void EmptyStringYieldsNoTokens()
        {
            var innerSplitter = new Mock<IWordSplitter>();
            var splitter = new XmlWordSplitter(innerSplitter.Object);
            Assert.AreEqual(0, splitter.SplitWords(string.Empty).Count());
            innerSplitter.Verify(s => s.SplitWords(It.Is<IEnumerable<string>>(a => a.Count() == 0)), Times.Exactly(1));
        }

        /// <summary>
        /// Tests that an xml string that only contains nodes, and no text, yields no words.
        /// </summary>
        [TestMethod]
        public void EmptyXmlNodesYieldsNoTokens()
        {
            var innerSplitter = new Mock<IWordSplitter>();
            var splitter = new XmlWordSplitter(innerSplitter.Object);
            Assert.AreEqual(0, splitter.SplitWords("<Test><Test2/><Test3 item='name' /><Test4 item=\"name\"></Test4></Test>").Count());
        }

        /// <summary>
        /// Any text contained within nodes should be passed to the inner splitter and yielded.
        /// </summary>
        [TestMethod]
        public void ShouldPassInnerTextToInnerSplitter()
        {
            var innerSplitter = new WordSplitter();
            var splitter = new XmlWordSplitter(innerSplitter);

            //innerSplitter.Setup(s => s.SplitWords("w1 word2")).Returns(new[] { new SplitWord("w1", 0), new SplitWord("word2", 1) });
            //innerSplitter.Setup(s => s.SplitWords("w3")).Returns(new[] { new SplitWord("w3", 0) });
            //innerSplitter.Setup(s => s.SplitWords("word4 5")).Returns(new[] { new SplitWord("word4", 0), new SplitWord("5", 1) });

            var results = splitter.SplitWords("<xml>w1 word2<inner>w3</inner><inner2>word4 5</inner2></xml>");
            Assert.IsTrue((new[] { "W1", "WORD2", "W3", "WORD4", "5" }).SequenceEqual(results.Select(w => w.Word)));
        }

        /// <summary>
        /// Any text contained within nodes should be passed to the inner splitter and yielded. Duplicate words should only
        /// be yielded once, even if they are found within multiple nodes.
        /// </summary>
        [TestMethod]
        public void ShouldCombineLocatedWords()
        {
            var innerSplitter = new WordSplitter();
            var splitter = new XmlWordSplitter(innerSplitter);

            //innerSplitter.Setup(s => s.SplitWords(new[] { "w1 w1", "w1", "w3", "word4 w1" })).Returns(new[] { new SplitWord("w1", new[] { 0, 1, 2,  }) });
            //innerSplitter.Setup(s => s.SplitWords("w1")).Returns(new[] { new SplitWord("w1", 0) });
            //innerSplitter.Setup(s => s.SplitWords("w3")).Returns(new[] { new SplitWord("w3", 0) });
            //innerSplitter.Setup(s => s.SplitWords("word4 w1")).Returns(new[] { new SplitWord("word4", 0), new SplitWord("w1", 1) });

            var results = splitter.SplitWords("<xml>w1 w1<inner>w1</inner>w3<inner2>word4 w1</inner2></xml>");
            Assert.IsTrue((new[] { "W1", "W3", "WORD4" }).SequenceEqual(results.Select(w => w.Word)));
            Assert.IsTrue((new[] { 0, 1, 2, 5 }).SequenceEqual(results.First().GetLocations()));
        }
    }
}

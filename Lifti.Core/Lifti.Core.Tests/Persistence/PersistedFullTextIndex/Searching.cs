// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using System.Linq;

    using Lifti.Extensibility;
    using Lifti.Persistence;
    using Lifti.Querying;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for searching a persisted full text index.
    /// </summary>
    [TestFixture]
    public class Searching
    {
        /// <summary>
        /// An index should return no results if it is empty.
        /// </summary>
        [Test]
        public void EmptyIndexShouldReturnNoResults()
        {
            var entryManager = new Mock<IPersistedEntryManager<int>>(MockBehavior.Strict);
            entryManager.Setup(m => m.Initialize());
            entryManager.SetupGet(m => m.ItemCount).Returns(0);
            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();

            var index = new PersistedFullTextIndex<int>(entryManager.Object, extensibilityService.Object);

            Assert.AreEqual(0, index.Count);

            entryManager.VerifyAll();
        }

        /// <summary>
        /// When nodes are accessed during a query execution, nodes should be lazy loaded from the persistence store.
        /// </summary>
        [Test]
        public void ShouldLazyLoadNodesOnDemand()
        {
            var entryManager = new Mock<IPersistedEntryManager<int>>(MockBehavior.Strict);
            entryManager.Setup(m => m.Initialize());
            entryManager.Setup(m => m.GetItemForId(2)).Returns(23);
            entryManager.Setup(m => m.GetItemForId(4)).Returns(11);
            entryManager.Setup(m => m.GetItemForId(7)).Returns(8);
            entryManager.Setup(m => m.GetItemForId(8)).Returns(277);
            entryManager.Setup(m => m.GetItemForId(9)).Returns(99);
            entryManager.Setup(m => m.GetItemForId(11)).Returns(29);

            entryManager.Setup(m => m.GetIndexNodeEntries(0)).Returns(new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(0, 4, 't') });
            entryManager.Setup(m => m.GetIndexNodeEntries(4)).Returns(
                new IndexNodeEntryBase[]
                    {
                        new ItemReferenceIndexNodeEntry(4, 2, 4), 
                        new ItemReferenceIndexNodeEntry(4, 8, 9), 
                        new NodeReferenceIndexNodeEntry(4, 5, 'a')
                    });

            entryManager.Setup(m => m.GetIndexNodeEntries(5)).Returns(
                new IndexNodeEntryBase[]
                    {
                        new NodeReferenceIndexNodeEntry(5, 12, 'b'), 
                        new ItemReferenceIndexNodeEntry(5, 9, 2), 
                        new ItemReferenceIndexNodeEntry(5, 4, 9),
                        new ItemReferenceIndexNodeEntry(5, 4, 12), 
                        new NodeReferenceIndexNodeEntry(5, 9, 'n')
                    });

            entryManager.Setup(m => m.GetIndexNodeEntries(9)).Returns(new IndexNodeEntryBase[] { new ItemReferenceIndexNodeEntry(9, 11, 2) });
            entryManager.Setup(m => m.GetIndexNodeEntries(12)).Returns(new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(12, 13, 'l') });
            entryManager.Setup(m => m.GetIndexNodeEntries(13)).Returns(new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(13, 14, 'e') });
            entryManager.Setup(m => m.GetIndexNodeEntries(14)).Returns(new IndexNodeEntryBase[] { new ItemReferenceIndexNodeEntry(14, 7, 9), new ItemReferenceIndexNodeEntry(14, 2, 9) });

            var index = new PersistedFullTextIndex<int>(entryManager.Object, new Mock<IIndexExtensibilityService<int>>().Object);

            // Verify the items haven been restored against the correct text
            Assert.IsNull(index.RootNode.Parent);
            Assert.AreSame(index.RootNode, index.RootNode.Match('t').Parent);

            Assert.IsTrue((new[] { 23, 277 }).SequenceEqual(index.Search(new FullTextQuery(new ExactWordQueryPart("t"))).OrderBy(i => i)));
            Assert.IsTrue((new[] { 11, 99 }).SequenceEqual(index.Search(new FullTextQuery(new ExactWordQueryPart("ta"))).OrderBy(i => i)));
            Assert.IsTrue((new[] { 29 }).SequenceEqual(index.Search(new FullTextQuery(new ExactWordQueryPart("tan"))).OrderBy(i => i)));
            Assert.IsTrue((new[] { 8, 23 }).SequenceEqual(index.Search(new FullTextQuery(new ExactWordQueryPart("table"))).OrderBy(i => i)));

            // Verify that 11 has been indexed against "ta" at 2 different word locations
            Assert.IsTrue((new[] { 9, 12 }).SequenceEqual(index.RootNode.Match('t').Match('a').GetDirectItems().First(m => m.Item == 11).Positions));

            entryManager.VerifyAll();
            entryManager.Verify(m => m.GetIndexNodeEntries(0), Times.Exactly(1));
            entryManager.Verify(m => m.GetIndexNodeEntries(4), Times.Exactly(1));
            entryManager.Verify(m => m.GetIndexNodeEntries(5), Times.Exactly(1));
            entryManager.Verify(m => m.GetIndexNodeEntries(9), Times.Exactly(1));
            entryManager.Verify(m => m.GetIndexNodeEntries(12), Times.Exactly(1));
            entryManager.Verify(m => m.GetIndexNodeEntries(13), Times.Exactly(1));
            entryManager.Verify(m => m.GetIndexNodeEntries(14), Times.Exactly(1));
        }
    }
}

// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for getting all item entries from the <see cref="PersistedEntryManager{TKey}"/> class.
    /// </summary>
    [TestFixture]
    public class GettingIndexNodeEntries : PersistedEntryManagerTestBase
    {
        /// <summary>
        /// If the index is empty, then no results should be returned for the root node (node 0).
        /// </summary>
        [Test]
        public void ShouldYieldNoResultsForNodeZeroInEmptyIndex()
        {
            var pageManager = CreateMockedPageManager();
            var headers = new[]
            {
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 0, 0, 0, Data.PageHeaderSize)
            };

            pageManager.Setup(p => p.GetPage(headers[0])).Returns(new IndexNodeDataPage(headers[0], new IndexNodeEntryBase[0]));

            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(headers));
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            Assert.AreEqual(0, manager.GetIndexNodeEntries(0).Count());
        }

        /// <summary>
        /// If the index contains entries for a node within a single page, all relevant entries should be returned.
        /// </summary>
        [Test]
        public void ShouldReturnEntriesForNodeWhenAllEntriesContainedInSinglePage()
        {
            var pageManager = CreateMockedPageManager();
            var headers = new[]
            {
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 5, 0, 2, Data.PageHeaderSize + 10 + 10 + 10 + 13 + 10)
            };

            var page0Entries = new IndexNodeEntryBase[] 
            {
                new ItemReferenceIndexNodeEntry(0, 3, 5),
                new ItemReferenceIndexNodeEntry(1, 3, 5),
                new ItemReferenceIndexNodeEntry(1, 4, 5),
                new NodeReferenceIndexNodeEntry(1, 3, 'b'),
                new ItemReferenceIndexNodeEntry(2, 3, 5)
            };

            pageManager.Setup(p => p.GetPage(headers[0])).Returns(new IndexNodeDataPage(headers[0], page0Entries));

            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(headers));
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            Assert.IsTrue((new[] { page0Entries[1], page0Entries[2], page0Entries[3] }).SequenceEqual(manager.GetIndexNodeEntries(1)));
        }

        /// <summary>
        /// If the index contains entries for a node within multiple pages, all relevant entries should be returned.
        /// </summary>
        [Test]
        public void ShouldReturnEntriesForNodeWhenAllEntriesContainedOverMultiplePages()
        {
            var pageManager = this.SetupMultipageNodeCollectionPageManager();
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            Assert.IsTrue((new[] { this.IndexNodePage0Entries[1], this.IndexNodePage1Entries[0], this.IndexNodePage1Entries[1], this.IndexNodePage1Entries[2], this.IndexNodePage2Entries[0] }).SequenceEqual(manager.GetIndexNodeEntries(1)));
        }
    }
}

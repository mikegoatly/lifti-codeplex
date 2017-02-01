// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using System.Linq;

    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for removing index node entries.
    /// </summary>
    [TestClass]
    public class RemovingIndexNodeEntry : PersistedEntryManagerTestBase
    {
        /// <summary>
        /// When removing an index node reference, the entry should be removed from the page.
        /// </summary>
        [TestMethod]
        public void ShouldRemoveItemReferenceFromPage()
        {
            var pageManager = CreateMockedPageManager();
            var indexNodePages = CreateTestIndexNodePages(pageManager);
            var itemNodePages = CreateTestItemNodeIndexPages(pageManager);

            pageManager.Setup(m => m.SavePage(indexNodePages[0]));
            pageManager.Setup(m => m.SavePage(itemNodePages[0]));

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            manager.RemoveNodeItemEntry(6, 99);

            // Verify the node to item relationships
            Assert.AreEqual(2, indexNodePages[0].Entries.Count());
            Assert.IsTrue((new[] { 5, 6 }).SequenceEqual(indexNodePages[0].Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 66, 99 }).SequenceEqual(indexNodePages[0].Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { IndexNodeEntryType.ItemReference, IndexNodeEntryType.NodeReference }).SequenceEqual(indexNodePages[0].Entries.Select(e => e.EntryType)));
            Assert.AreEqual(3, indexNodePages[1].Entries.Count());

            // Verify the reverse relationship from item to node
            Assert.AreEqual(1, itemNodePages[0].Entries.Count());
            Assert.AreEqual(66, itemNodePages[0].Entries.Single().Id);
            Assert.AreEqual(5, itemNodePages[0].Entries.Single().ReferencedId);
            Assert.AreEqual(2, itemNodePages[1].Entries.Count());
            
            pageManager.Verify(m => m.SavePage(indexNodePages[0]), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(itemNodePages[0]), Times.Exactly(1));
        }

        /// <summary>
        /// When removing an index node reference, the entry should be removed from the page.
        /// </summary>
        [TestMethod]
        public void ShouldRemoveNodeReferenceFromPage()
        {
            var pageManager = CreateMockedPageManager(); 
            var indexNodePages = CreateTestIndexNodePages(pageManager);

            pageManager.Setup(m => m.SavePage(indexNodePages[0]));

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            manager.RemoveIndexNodeReferenceEntry(6, 99);

            Assert.AreEqual(3, indexNodePages[0].Entries.Count());
            Assert.IsTrue((new[] { 5, 6, 6 }).SequenceEqual(indexNodePages[0].Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 66, 99, 99 }).SequenceEqual(indexNodePages[0].Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { IndexNodeEntryType.ItemReference, IndexNodeEntryType.ItemReference, IndexNodeEntryType.ItemReference }).SequenceEqual(indexNodePages[0].Entries.Select(e => e.EntryType)));
            Assert.AreEqual(3, indexNodePages[1].Entries.Count());

            pageManager.Verify(m => m.SavePage(indexNodePages[0]), Times.Exactly(1));
        }

        /// <summary>
        /// Creates the index node pages.
        /// </summary>
        /// <param name="pageManager">The page manager.</param>
        /// <returns>
        /// The test pages.
        /// </returns>
        private static IndexNodeDataPage[] CreateTestIndexNodePages(Mock<IPageManager> pageManager)
        {
            var pages = new[]
                {
                    new IndexNodeDataPage(
                        new DataPageHeader(DataPageType.IndexNode, 3, 1, 4, 4, 5, 6, Data.PageHeaderSize + 30), 
                        new IndexNodeEntryBase[] { new ItemReferenceIndexNodeEntry(5, 66, 6), new ItemReferenceIndexNodeEntry(6, 99, 20), new NodeReferenceIndexNodeEntry(6, 99, '4'), new ItemReferenceIndexNodeEntry(6, 99, 2) }),
                    new IndexNodeDataPage(
                        new DataPageHeader(DataPageType.IndexNode, 4, 3, 7, 3, 6, 8, Data.PageHeaderSize + 30), 
                        new IndexNodeEntryBase[] { new ItemReferenceIndexNodeEntry(6, 12, 3), new NodeReferenceIndexNodeEntry(6, 12, '4'), new ItemReferenceIndexNodeEntry(8, 99, 2) })
                };

            pageManager.Setup(m => m.IndexNodeDataPages.FindPagesForEntry(6)).Returns(pages.Select(p => p.Header));
            pageManager.Setup(m => m.GetPage(pages[0].Header)).Returns(pages[0]);
            pageManager.Setup(m => m.GetPage(pages[1].Header)).Returns(pages[1]);

            return pages;
        }

        /// <summary>
        /// Creates the test item node index pages.
        /// </summary>
        /// <param name="pageManager">The page manager.</param>
        /// <returns>
        /// The test pages.
        /// </returns>
        private static ItemNodeIndexDataPage[] CreateTestItemNodeIndexPages(Mock<IPageManager> pageManager)
        {
            var pages = new[]
                {
                    new ItemNodeIndexDataPage(
                        new DataPageHeader(DataPageType.ItemNodeIndex, 5, 1, 4, 2, 5, 6, Data.PageHeaderSize + 16), 
                        new[] { new ItemNodeIndexEntry(66, 5), new ItemNodeIndexEntry(99, 6) }),
                    new ItemNodeIndexDataPage(
                        new DataPageHeader(DataPageType.ItemNodeIndex, 6, 3, 7, 2, 6, 8, Data.PageHeaderSize + 16), 
                        new[] { new ItemNodeIndexEntry(12, 6), new ItemNodeIndexEntry(99, 8) })
                };

            pageManager.Setup(m => m.ItemNodeIndexDataPages.FindPagesForEntry(99)).Returns(pages.Select(p => p.Header));
            pageManager.Setup(m => m.GetPage(pages[0].Header)).Returns(pages[0]);
            pageManager.Setup(m => m.GetPage(pages[1].Header)).Returns(pages[1]);

            return pages;
        }
    }
}

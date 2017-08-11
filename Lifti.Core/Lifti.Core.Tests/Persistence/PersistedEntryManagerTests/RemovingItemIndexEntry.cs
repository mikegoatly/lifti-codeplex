// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the removal of item index entries.
    /// </summary>
    [TestFixture]
    public class RemovingItemIndexEntry : PersistedEntryManagerTestBase
    {
        /// <summary>
        /// When removing an index node reference, the entry should be removed from the page.
        /// </summary>
        [Test]
        public void ShouldRemoveItemReferenceFromPage()
        {
            var pageManager = CreateMockedPageManager();

            // Mock the item page
            var itemPage = new ItemIndexDataPage<int>(
                new DataPageHeader(DataPageType.Items, 3, 1, 4, 4, 5, 6, Data.PageHeaderSize + (8 * 4)),
                new[] { new ItemEntry<int>(0, 66, 8), new ItemEntry<int>(1, 99, 8), new ItemEntry<int>(2, 92, 8), new ItemEntry<int>(3, 9, 8) });

            pageManager.Setup(m => m.ItemDataPages.FindPagesForEntry(2)).Returns(new[] { itemPage.Header });
            pageManager.Setup(m => m.ItemDataPages.GetEnumerator()).Returns((new[] { itemPage.Header }).OfType<IDataPageHeader>().GetEnumerator());
            pageManager.Setup(m => m.GetPage(itemPage.Header)).Returns(itemPage);

            // Mock the item node index page
            var itemNodePage = new ItemNodeIndexDataPage(
                new DataPageHeader(DataPageType.ItemNodeIndex, 3, 1, 4, 4, 5, 6, Data.PageHeaderSize + (8 * 4)),
                new[] { new ItemNodeIndexEntry(1, 5), new ItemNodeIndexEntry(2, 8), new ItemNodeIndexEntry(2, 11), new ItemNodeIndexEntry(3, 1) });

            pageManager.Setup(m => m.ItemNodeIndexDataPages.FindPagesForEntry(2)).Returns(new[] { itemNodePage.Header });
            pageManager.Setup(m => m.GetPage(itemNodePage.Header)).Returns(itemNodePage);

            // Mock the node index pages
            var indexNodePage1 = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 3, 1, 4, 4, 5, 6, Data.PageHeaderSize + (13 * 3)),
                new[] { new ItemReferenceIndexNodeEntry(7, 3, 1), new ItemReferenceIndexNodeEntry(8, 2, 7), new ItemReferenceIndexNodeEntry(8, 2, 99) });
            var indexNodePage2 = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 3, 1, 4, 4, 5, 6, Data.PageHeaderSize + (13 * 3)),
                new[] { new ItemReferenceIndexNodeEntry(9, 3, 1), new ItemReferenceIndexNodeEntry(11, 2, 7), new ItemReferenceIndexNodeEntry(12, 9, 99) });

            pageManager.Setup(m => m.IndexNodeDataPages.FindPagesForEntry(8)).Returns(new[] { indexNodePage1.Header });
            pageManager.Setup(m => m.IndexNodeDataPages.FindPagesForEntry(11)).Returns(new[] { indexNodePage2.Header });
            pageManager.Setup(m => m.GetPage(indexNodePage1.Header)).Returns(indexNodePage1);
            pageManager.Setup(m => m.GetPage(indexNodePage2.Header)).Returns(indexNodePage2);

            pageManager.Setup(m => m.SavePage(itemPage));
            pageManager.Setup(m => m.SavePage(itemNodePage));
            pageManager.Setup(m => m.SavePage(indexNodePage1));
            pageManager.Setup(m => m.SavePage(indexNodePage2));

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);
            manager.Initialize();

            manager.RemoveItemEntry(2);

            Assert.AreEqual(3, itemPage.Entries.Count());
            Assert.IsTrue((new[] { 0, 1, 3 }).SequenceEqual(itemPage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 66, 99, 9 }).SequenceEqual(itemPage.Entries.Select(e => e.Item)));

            Assert.IsTrue((new[] { 1, 3 }).SequenceEqual(itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 1 }).SequenceEqual(itemNodePage.Entries.Select(e => e.ReferencedId)));

            Assert.IsTrue((new[] { 7 }).SequenceEqual(indexNodePage1.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 3 }).SequenceEqual(indexNodePage1.Entries.Select(e => e.ReferencedId)));

            Assert.IsTrue((new[] { 9, 12 }).SequenceEqual(indexNodePage2.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 3, 9 }).SequenceEqual(indexNodePage2.Entries.Select(e => e.ReferencedId)));

            pageManager.Verify(m => m.SavePage(itemPage), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(itemNodePage), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(indexNodePage1), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(indexNodePage2), Times.Exactly(1));
        }
    }
}

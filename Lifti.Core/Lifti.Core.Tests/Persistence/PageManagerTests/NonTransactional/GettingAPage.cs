// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.NonTransactional
{
    using System.Linq;

    using Lifti.Persistence;
    using Lifti.Tests.Persistence.PageManagerTests.Setup;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for getting pages from the page manager.
    /// </summary>
    [TestClass]
    public class GettingAPage : PageManagerTestBase
    {
        /// <summary>
        /// Tests that an item data page is successfully fetched.
        /// </summary>
        [TestMethod]
        public void ShouldReturnItemDataPageWhenRequested()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, null, null)
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 100).AndItem(4, 200).AndItem(8, 600))
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(1);

            // Verify
            Assert.IsInstanceOfType(page, typeof(ItemIndexDataPage<int>));
            var dataPage = (ItemIndexDataPage<int>)page;

            Assert.AreEqual(3, dataPage.Entries.Count());
            Assert.AreEqual(1, dataPage.Header.FirstEntry);
            Assert.AreEqual(8, dataPage.Header.LastEntry);

            Assert.IsTrue((new[] { 1, 4, 8 }).SequenceEqual(dataPage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 100, 200, 600 }).SequenceEqual(dataPage.Entries.Select(e => e.Item)));
        }

        /// <summary>
        /// Tests that an index node data page is successfully fetched.
        /// </summary>
        [TestMethod]
        public void ShouldReturnIndexNodeDataPageWhenRequested()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, null, new ItemRefSetup(5, 5, 4).AndItemRef(5, 8, 5).AndNodeRef(5, 9, 'a').AndItemRef(9, 5, 2).AndItemRef(9, 56, 9).AndNodeRef(11, 8, 'b'))
                                .ItemPage<int>(1, null, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(0);

            // Verify
            Assert.IsInstanceOfType(page, typeof(IndexNodeDataPage));
            var dataPage = (IndexNodeDataPage)page;

            Assert.AreEqual(6, dataPage.Entries.Count());
            Assert.AreEqual(5, dataPage.Header.FirstEntry);
            Assert.AreEqual(11, dataPage.Header.LastEntry);

            Assert.IsTrue((new[] { 5, 5, 5, 9, 9, 11 }).SequenceEqual(dataPage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 8, 9, 5, 56, 8 }).SequenceEqual(dataPage.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { IndexNodeEntryType.ItemReference, IndexNodeEntryType.ItemReference, IndexNodeEntryType.NodeReference, IndexNodeEntryType.ItemReference, IndexNodeEntryType.ItemReference, IndexNodeEntryType.NodeReference }).SequenceEqual(dataPage.Entries.Select(e => e.EntryType)));
            Assert.IsTrue((new[] { 'a', 'b' }).SequenceEqual(dataPage.Entries.OfType<NodeReferenceIndexNodeEntry>().Select(e => e.MatchedCharacter)));
            Assert.IsTrue((new[] { 4, 5, 2, 9 }).SequenceEqual(dataPage.Entries.OfType<ItemReferenceIndexNodeEntry>().Select(e => e.MatchPosition)));
        }
    }
}

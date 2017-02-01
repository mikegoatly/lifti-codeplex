// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using System.Linq;

    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for adding an index node entry to the entry manager.
    /// </summary>
    [TestClass]
    public class AddingIndexNodeEntry : PersistedEntryManagerTestBase
    {
        /// <summary>
        /// The maximum number of index item entries that can appear on a page.
        /// </summary>
        private const short MaxItemsPerPage = (Data.PageSize - Data.PageHeaderSize) / 10;

        /// <summary>
        /// The index page used in certain tests.
        /// </summary>
        private IndexNodeDataPage indexPage;

        /// <summary>
        /// The item node index page used in certain tests
        /// </summary>
        private ItemNodeIndexDataPage itemNodePage;

        /// <summary>
        /// If an item node index entry already exists, linking the item and the node, a duplicate
        /// entry should not be created and no exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ItemNodeIndexEntryShouldNotBeInsertedIfItAlreadyExists()
        {
            var pageManager = this.SetupMultipageNodeCollectionPageManager();
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            this.CreateItemNodeIndexForPages(pageManager, this.IndexNodePage0, this.IndexNodePage1, this.IndexNodePage2);

            pageManager.Setup(m => m.SavePage(this.IndexNodePage2));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(3, 99, 12);

            // Add another entry, at a different word position
            manager.AddNodeItemEntry(3, 99, 11);

            Assert.IsTrue((new[] { 1, 3, 3, 4 }).SequenceEqual(this.IndexNodePage2.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 8, 99, 99, 8 }).SequenceEqual(this.IndexNodePage2.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 5, 11, 12, 9 }).SequenceEqual(this.IndexNodePage2.Entries.OfType<ItemReferenceIndexNodeEntry>().Select(e => e.MatchPosition)));

            Assert.IsTrue((new[] { 3, 3, 4, 8, 8, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 1, 1, 1, 4, 3 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));

            pageManager.Verify(m => m.SavePage(this.IndexNodePage0), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage1), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage2), Times.Exactly(2));
            pageManager.Verify(m => m.SavePage(this.itemNodePage), Times.Exactly(1));
        }

        /// <summary>
        /// When an item entry is inserted and it fits into an existing page, it should be inserted into the correct location.
        /// </summary>
        [TestMethod]
        public void NodeEntryShouldBeInsertedIntoMidPageWhenItFits()
        {
            var pageManager = this.SetupMultipageNodeCollectionPageManager();
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            pageManager.Setup(m => m.SavePage(this.IndexNodePage2));

            manager.AddIndexNodeReferenceEntry(3, 99, 'b');

            Assert.IsTrue((new[] { 1, 3, 4 }).SequenceEqual(this.IndexNodePage2.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 8, 99, 8 }).SequenceEqual(this.IndexNodePage2.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 5, 9 }).SequenceEqual(this.IndexNodePage2.Entries.OfType<ItemReferenceIndexNodeEntry>().Select(e => e.MatchPosition)));
            Assert.IsTrue((new[] { 'b' }).SequenceEqual(this.IndexNodePage2.Entries.OfType<NodeReferenceIndexNodeEntry>().Select(e => e.MatchedCharacter)));

            pageManager.Verify(m => m.SavePage(this.IndexNodePage0), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage1), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage2), Times.Exactly(1));
        }

        /// <summary>
        /// When an item entry is inserted and it fits into an existing page, it should be inserted into the correct location.
        /// </summary>
        [TestMethod]
        public void ItemEntryShouldBeInsertedIntoMidPageWhenItFits()
        {
            var pageManager = this.SetupMultipageNodeCollectionPageManager();
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            this.CreateItemNodeIndexForPages(pageManager, this.IndexNodePage0, this.IndexNodePage1, this.IndexNodePage2);

            pageManager.Setup(m => m.SavePage(this.IndexNodePage2));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(3, 99, 12);

            Assert.IsTrue((new[] { 1, 3, 4 }).SequenceEqual(this.IndexNodePage2.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 8, 99, 8 }).SequenceEqual(this.IndexNodePage2.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 5, 12, 9 }).SequenceEqual(this.IndexNodePage2.Entries.OfType<ItemReferenceIndexNodeEntry>().Select(e => e.MatchPosition)));

            Assert.IsTrue((new[] { 3, 3, 4, 8, 8, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 1, 1, 1, 4, 3 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));

            pageManager.Verify(m => m.SavePage(this.IndexNodePage0), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage1), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage2), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(this.itemNodePage), Times.Exactly(1));
        }

        /// <summary>
        /// When an item entry is inserted and it fits into an existing page, it should be inserted into the correct location.
        /// </summary>
        [TestMethod]
        public void ItemEntryShouldBeInsertedAtEndOPageWhenItFits()
        {
            var pageManager = this.SetupMultipageNodeCollectionPageManager();
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            this.CreateItemNodeIndexForPages(pageManager, this.IndexNodePage0, this.IndexNodePage1, this.IndexNodePage2);

            pageManager.Setup(m => m.SavePage(this.IndexNodePage2));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(5, 99, 12);

            Assert.IsTrue((new[] { 1, 4, 5 }).SequenceEqual(this.IndexNodePage2.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 8, 8, 99 }).SequenceEqual(this.IndexNodePage2.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 5, 9, 12 }).SequenceEqual(this.IndexNodePage2.Entries.OfType<ItemReferenceIndexNodeEntry>().Select(e => e.MatchPosition)));

            Assert.IsTrue((new[] { 3, 3, 4, 8, 8, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 1, 1, 1, 4, 5 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));

            pageManager.Verify(m => m.SavePage(this.IndexNodePage0), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage1), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage2), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(this.itemNodePage), Times.Exactly(1));
        }

        /// <summary>
        /// When an item is inserted with an id that already exists across multiple pages, it should be inserted into the lowest possible
        /// page.
        /// </summary>
        [TestMethod]
        public void ItemEntryShouldBeInsertedIntoLowestPossiblePageWhenItFits()
        {
            var pageManager = this.SetupMultipageNodeCollectionPageManager();
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            this.CreateItemNodeIndexForPages(pageManager, this.IndexNodePage0, this.IndexNodePage1, this.IndexNodePage2);

            pageManager.Setup(m => m.SavePage(this.IndexNodePage0));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(1, 99, 12);

            Assert.IsTrue((new[] { 0, 1, 1 }).SequenceEqual(this.IndexNodePage0.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 3, 3, 99 }).SequenceEqual(this.IndexNodePage0.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 5, 5, 12 }).SequenceEqual(this.IndexNodePage0.Entries.OfType<ItemReferenceIndexNodeEntry>().Select(e => e.MatchPosition)));

            Assert.IsTrue((new[] { 3, 3, 4, 8, 8, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 1, 1, 1, 4, 1 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));

            pageManager.Verify(m => m.SavePage(this.IndexNodePage0), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(this.IndexNodePage1), Times.Never());
            pageManager.Verify(m => m.SavePage(this.IndexNodePage2), Times.Never());
            pageManager.Verify(m => m.SavePage(this.itemNodePage), Times.Exactly(1));
        }

        /// <summary>
        /// If an item is inserted and it doesn't fit on the first available page, it should be added to the next page if it fits there.
        /// </summary>
        [TestMethod]
        public void IfItemWillNotFitOnPageButFitsOnNextPageItShouldBeInsertedThere()
        {
            var pageManager = CreateMockedPageManager();
            var indexNodeHeaders = new[]
                {
                    new DataPageHeader(DataPageType.IndexNode, 0, null, 1, MaxItemsPerPage, 0, 0, Data.PageHeaderSize + (MaxItemsPerPage * 10)),
                    new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 2, 1, 1, Data.PageHeaderSize + 10 + 13)
                };

            var page0Entries = Enumerable.Repeat(new ItemReferenceIndexNodeEntry(0, 3, 5), MaxItemsPerPage).ToArray();
            var page1Entries = new IndexNodeEntryBase[]
                {
                    new ItemReferenceIndexNodeEntry(1, 4, 5),
                    new NodeReferenceIndexNodeEntry(1, 3, 'b')
                };

            var page0 = new IndexNodeDataPage(indexNodeHeaders[0], page0Entries);
            var page1 = new IndexNodeDataPage(indexNodeHeaders[1], page1Entries);

            this.CreateItemNodeIndexForPages(pageManager, page0, page1);

            pageManager.Setup(p => p.GetPage(indexNodeHeaders[0])).Returns(page0);
            pageManager.Setup(p => p.GetPage(1)).Returns(page1);

            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(indexNodeHeaders));

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            pageManager.Setup(m => m.SavePage(page1));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(0, 99, 12);

            Assert.IsTrue((new[] { 0, 1, 1 }).SequenceEqual(page1.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 99, 4, 3 }).SequenceEqual(page1.Entries.Select(e => e.ReferencedId)));

            Assert.IsTrue((new[] { 3, 4, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 1, 0 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));
        }

        /// <summary>
        /// If an item is inserted and it doesn't fit on the first available page, it should be added to the next appropriate page if it fits there.
        /// </summary>
        [TestMethod]
        public void IfItemWillNotFitOnPageButFitsOnLastPageStartingWithIdsItShouldBeInsertedThere()
        {
            var pageManager = CreateMockedPageManager();
            var indexNodeHeaders = new[]
                {
                    new DataPageHeader(DataPageType.IndexNode, 0, null, 1, MaxItemsPerPage, 0, 0, Data.PageHeaderSize + (MaxItemsPerPage * 10)),
                    new DataPageHeader(DataPageType.IndexNode, 1, 0, 2, MaxItemsPerPage, 0, 0, Data.PageHeaderSize + (MaxItemsPerPage * 10)),
                    new DataPageHeader(DataPageType.IndexNode, 2, 1, null, 2, 1, 1, Data.PageHeaderSize + 10 + 13)
                };

            var page0Entries = Enumerable.Repeat(new ItemReferenceIndexNodeEntry(0, 3, 5), MaxItemsPerPage).ToArray();
            var page1Entries = Enumerable.Repeat(new ItemReferenceIndexNodeEntry(0, 5, 7), MaxItemsPerPage).ToArray();
            var page2Entries = new IndexNodeEntryBase[]
                {
                    new ItemReferenceIndexNodeEntry(1, 4, 5),
                    new NodeReferenceIndexNodeEntry(1, 3, 'b')
                };

            var page0 = new IndexNodeDataPage(indexNodeHeaders[0], page0Entries);
            var page1 = new IndexNodeDataPage(indexNodeHeaders[1], page1Entries);
            var page2 = new IndexNodeDataPage(indexNodeHeaders[2], page2Entries);

            this.CreateItemNodeIndexForPages(pageManager, page0, page1, page2);

            pageManager.Setup(p => p.GetPage(indexNodeHeaders[0])).Returns(page0);
            pageManager.Setup(p => p.GetPage(1)).Returns(page1);
            pageManager.Setup(p => p.GetPage(2)).Returns(page2);

            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(indexNodeHeaders));

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            pageManager.Setup(m => m.SavePage(page2));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(0, 99, 12);

            Assert.IsTrue((new[] { 0, 1, 1 }).SequenceEqual(page2.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 99, 4, 3 }).SequenceEqual(page2.Entries.Select(e => e.ReferencedId)));

            Assert.IsTrue((new[] { 3, 4, 5, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 1, 0, 0 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));
        }

        /// <summary>
        /// If an item is inserted and it doesn't fit on the first available page, it should be added to the next appropriate page if it fits there.
        /// </summary>
        [TestMethod]
        public void IfItemWillNotFitOnPageButFitsOnNextPageStartingWithIdsItShouldBeInsertedThere()
        {
            var pageManager = CreateMockedPageManager();
            var indexNodeHeaders = new[]
                {
                    new DataPageHeader(DataPageType.IndexNode, 0, null, 1, MaxItemsPerPage, 0, 0, Data.PageHeaderSize + (MaxItemsPerPage * 10)),
                    new DataPageHeader(DataPageType.IndexNode, 1, 0, 2, 1, 0, 0, Data.PageHeaderSize + 10),
                    new DataPageHeader(DataPageType.IndexNode, 2, 1, null, MaxItemsPerPage, 0, 0, Data.PageHeaderSize + (MaxItemsPerPage * 10))
                };

            var page0Entries = Enumerable.Repeat(new ItemReferenceIndexNodeEntry(0, 3, 5), MaxItemsPerPage).ToArray();
            var page1Entries = new[] { new ItemReferenceIndexNodeEntry(0, 5, 7) };
            var page2Entries = Enumerable.Repeat(new ItemReferenceIndexNodeEntry(0, 8, 9), MaxItemsPerPage).ToArray();

            var page0 = new IndexNodeDataPage(indexNodeHeaders[0], page0Entries);
            var page1 = new IndexNodeDataPage(indexNodeHeaders[1], page1Entries);
            var page2 = new IndexNodeDataPage(indexNodeHeaders[2], page2Entries);

            this.CreateItemNodeIndexForPages(pageManager, page0, page1, page2);

            pageManager.Setup(p => p.GetPage(indexNodeHeaders[0])).Returns(page0);
            pageManager.Setup(p => p.GetPage(1)).Returns(page1);

            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(indexNodeHeaders));

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            pageManager.Setup(m => m.SavePage(page1));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(0, 99, 12);

            Assert.IsTrue((new[] { 0, 0 }).SequenceEqual(page1.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 99 }).SequenceEqual(page1.Entries.Select(e => e.ReferencedId)));

            Assert.IsTrue((new[] { 3, 5, 8, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 0, 0, 0 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));
        }

        /// <summary>
        /// If an item is inserted and it doesn't fit on the first available page, it should be added to the previous page if it fits there.
        /// </summary>
        [TestMethod]
        public void IfItemWillNotFitOnPageButFitsOnPreviousPageEndingWithIdItShouldBeInsertedThere()
        {
            var pageManager = CreateMockedPageManager();
            var indexNodeHeaders = new[]
                {
                    new DataPageHeader(DataPageType.IndexNode, 0, null, 1, MaxItemsPerPage, 0, 0, Data.PageHeaderSize + 10 + 13),
                    new DataPageHeader(DataPageType.IndexNode, 1, 0, null, MaxItemsPerPage, 1, 1, Data.PageHeaderSize + (MaxItemsPerPage * 10))
                };

            var page0Entries = new IndexNodeEntryBase[]
                {
                    new ItemReferenceIndexNodeEntry(0, 4, 5),
                    new NodeReferenceIndexNodeEntry(0, 3, 'b')
                };

            var page1Entries = Enumerable.Repeat(new ItemReferenceIndexNodeEntry(1, 5, 7), MaxItemsPerPage).ToArray();

            var page0 = new IndexNodeDataPage(indexNodeHeaders[0], page0Entries);
            var page1 = new IndexNodeDataPage(indexNodeHeaders[1], page1Entries);

            this.CreateItemNodeIndexForPages(pageManager, page0, page1);

            pageManager.Setup(p => p.GetPage(0)).Returns(page0);
            pageManager.Setup(p => p.GetPage(indexNodeHeaders[1])).Returns(page1);

            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(indexNodeHeaders));

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            pageManager.Setup(m => m.SavePage(page0));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(1, 99, 12);

            Assert.IsTrue((new[] { 0, 0, 1 }).SequenceEqual(page0.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 4, 3, 99 }).SequenceEqual(page0.Entries.Select(e => e.ReferencedId)));

            Assert.IsTrue((new[] { 4, 5, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 1, 1 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));
        }

        /// <summary>
        /// When a new item is added and it causes a page split, if its id is the same as all the ids on the old page, it should be the only
        /// entry on the new page.
        /// </summary>
        [TestMethod]
        public void SplittingAPageWhereItemHasSameIdAsAllOnPageShouldOnlyCauseNewItemToBeOnNewPage()
        {
            var pageManager = CreateMockedPageManager();
            this.SetupSimplePageSplitScenario(pageManager);

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            var createdPage = new IndexNodeDataPage(new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 0, 0, 0, Data.PageHeaderSize), new IndexNodeEntryBase[0]);
            pageManager.Setup(m => m.CreatePage((DataPage<IndexNodeEntryBase>)this.indexPage)).Returns(createdPage);
            pageManager.Setup(m => m.SavePage(createdPage));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(0, 99, 12);

            Assert.IsTrue((new[] { 0 }).SequenceEqual(createdPage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 99 }).SequenceEqual(createdPage.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 3, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 0 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));

            pageManager.Verify(m => m.SavePage(this.indexPage), Times.Never());
            pageManager.Verify(m => m.SavePage(createdPage), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(this.itemNodePage), Times.Exactly(1));
        }

        /// <summary>
        /// When a new item is added and it causes a page split, if its id is greater than the last id on the old page, it should be the only
        /// entry on the new page.
        /// </summary>
        [TestMethod]
        public void SplittingAPageWhereItemHasToBeAddedAtTheEndShouldOnlyCauseNewItemToBeOnNewPage()
        {
            var pageManager = CreateMockedPageManager();
            this.SetupSimplePageSplitScenario(pageManager);

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            var createPage = new IndexNodeDataPage(new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 0, 0, 0, Data.PageHeaderSize), new IndexNodeEntryBase[0]);
            pageManager.Setup(m => m.CreatePage((DataPage<IndexNodeEntryBase>)this.indexPage)).Returns(createPage);
            pageManager.Setup(m => m.SavePage(createPage));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(1, 99, 12);

            Assert.IsTrue((new[] { 1 }).SequenceEqual(createPage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 99 }).SequenceEqual(createPage.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 3, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 1 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));

            pageManager.Verify(m => m.SavePage(this.indexPage), Times.Never());
            pageManager.Verify(m => m.SavePage(createPage), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(this.itemNodePage), Times.Exactly(1));
        }

        /// <summary>
        /// When a full page split occurs (i.e. items are automatically moved onto the split page, and the first page ends up smaller,
        /// then the new item should be inserted onto the first page.
        /// </summary>
        [TestMethod]
        public void SplittingAPageWhereisertedMidPageShouldCauseItemToBeInsertedOnFirstPageWhenItIsSmaller()
        {
            var pageManager = CreateMockedPageManager();
            var indexNodeHeaders = new[]
                {
                    new DataPageHeader(DataPageType.IndexNode, 0, null, null, MaxItemsPerPage, 0, 2, Data.PageHeaderSize + (MaxItemsPerPage * 10))
                };

            var page0Entries = (new[] { new ItemReferenceIndexNodeEntry(0, 9, 2) }).Concat(Enumerable.Repeat(new ItemReferenceIndexNodeEntry(2, 3, 5), MaxItemsPerPage - 1)).ToArray();
            var page0 = new IndexNodeDataPage(indexNodeHeaders[0], page0Entries);
            pageManager.Setup(p => p.GetPage(indexNodeHeaders[0])).Returns(page0);
            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(indexNodeHeaders));

            this.CreateItemNodeIndexForPages(pageManager, page0);

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            var page1 = new IndexNodeDataPage(new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 0, 0, 0, Data.PageHeaderSize), new IndexNodeEntryBase[0]);
            pageManager.Setup(m => m.CreatePage((DataPage<IndexNodeEntryBase>)page0)).Returns(page1);
            pageManager.Setup(m => m.SavePage(page0));
            pageManager.Setup(m => m.SavePage(page1));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(1, 99, 12);

            Assert.IsTrue((new[] { 0, 1 }).SequenceEqual(page0.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 9, 99 }).SequenceEqual(page0.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 3, 9, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 2, 0, 1 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(MaxItemsPerPage - 1, page1.Entries.Count());
            Assert.IsTrue(page1.Entries.All(e => e.Id == 2));

            pageManager.Verify(m => m.SavePage(page0), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(page1), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(this.itemNodePage), Times.Exactly(1));
        }

        /// <summary>
        /// When a full page split occurs (i.e. items are automatically moved onto the split page, and the second page ends up smaller,
        /// then the new item should be inserted onto the first page.
        /// </summary>
        [TestMethod]
        public void SplittingAPageWhereItemInsertedMidPageShouldCauseItemToBeInsertedOnSecondPageWhenItIsSmaller()
        {
            var pageManager = CreateMockedPageManager();
            var indexNodeHeaders = new[]
                {
                    new DataPageHeader(DataPageType.IndexNode, 0, null, null, MaxItemsPerPage, 0, 2, Data.PageHeaderSize + (MaxItemsPerPage * 10))
                };

            var page0Entries = Enumerable.Repeat(new ItemReferenceIndexNodeEntry(0, 3, 5), MaxItemsPerPage - 1).Concat(new[] { new ItemReferenceIndexNodeEntry(2, 9, 2) }).ToArray();
            var page0 = new IndexNodeDataPage(indexNodeHeaders[0], page0Entries);
            pageManager.Setup(p => p.GetPage(indexNodeHeaders[0])).Returns(page0);
            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(indexNodeHeaders));

            this.CreateItemNodeIndexForPages(pageManager, page0);

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            var page1 = new IndexNodeDataPage(new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 0, 0, 0, Data.PageHeaderSize), new IndexNodeEntryBase[0]);
            pageManager.Setup(m => m.CreatePage((DataPage<IndexNodeEntryBase>)page0)).Returns(page1);
            pageManager.Setup(m => m.SavePage(page0));
            pageManager.Setup(m => m.SavePage(page1));
            pageManager.Setup(m => m.SavePage(this.itemNodePage));

            manager.AddNodeItemEntry(1, 99, 12);

            Assert.IsTrue((new[] { 1, 2 }).SequenceEqual(page1.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 99, 9 }).SequenceEqual(page1.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 3, 9, 99 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 0, 2, 1 }).SequenceEqual(this.itemNodePage.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(MaxItemsPerPage - 1, page0.Entries.Count());
            Assert.IsTrue(page0.Entries.All(e => e.Id == 0));

            pageManager.Verify(m => m.SavePage(page0), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(page1), Times.Exactly(1));
            pageManager.Verify(m => m.SavePage(this.itemNodePage), Times.Exactly(1));
        }

        /// <summary>
        /// Sets up a simple page split scenario.
        /// </summary>
        /// <param name="pageManager">The page manager.</param>
        private void SetupSimplePageSplitScenario(Mock<IPageManager> pageManager)
        {
            var indexNodeHeaders = new[]
                {
                    new DataPageHeader(DataPageType.IndexNode, 0, null, null, MaxItemsPerPage, 0, 0, Data.PageHeaderSize + (MaxItemsPerPage * 10))
                };

            var indexPageHeader = Enumerable.Repeat(new ItemReferenceIndexNodeEntry(0, 3, 5), MaxItemsPerPage).ToArray();
            this.indexPage = new IndexNodeDataPage(indexNodeHeaders[0], indexPageHeader);
            pageManager.Setup(p => p.GetPage(indexNodeHeaders[0])).Returns(this.indexPage);
            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(indexNodeHeaders));

            this.CreateItemNodeIndexForPages(pageManager, this.indexPage);
        }

        /// <summary>
        /// Creates an item node index page that mirrors the relationships in the given index pages.
        /// </summary>
        /// <param name="pageManager">The page manager.</param>
        /// <param name="pages">The pages to created the mirrored relationships for.</param>
        private void CreateItemNodeIndexForPages(Mock<IPageManager> pageManager, params IndexNodeDataPage[] pages)
        {
            var entries = (from p in pages
                           from e in p.Entries.OfType<ItemReferenceIndexNodeEntry>()
                           orderby e.ReferencedId
                           select new { e.ReferencedId, e.Id })
                           .Distinct()
                           .Select(e => new ItemNodeIndexEntry(e.ReferencedId, e.Id))
                           .ToArray();

            var itemNodePageHeader = new DataPageHeader(DataPageType.ItemNodeIndex, 2, null, null, entries.Length, entries.First().Id, entries.Last().Id, (short)(Data.PageHeaderSize + (8 * entries.Length)));
            this.itemNodePage = new ItemNodeIndexDataPage(itemNodePageHeader, entries);
            pageManager.SetupGet(p => p.ItemNodeIndexDataPages).Returns(new DataPageCollection(new[] { itemNodePageHeader }));
            pageManager.Setup(p => p.GetPage(itemNodePageHeader)).Returns(this.itemNodePage);
        }
    }
}

// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.NonTransactional
{
    using Lifti.Persistence;
    using Lifti.Tests.Persistence.PageManagerTests.Setup;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for getting pages from the page manager.
    /// </summary>
    [TestClass]
    public class InvalidatingAPage : PageManagerTestBase
    {
        /// <summary>
        /// The last item index page should not be invalidated - it should remain in the page manager.
        /// </summary>
        [TestMethod]
        public void ShouldLeaveTheLastItemIndexPageWhenInvalidated()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, null)
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 3))
                                .IndexNodePage(2, 0, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(1) as ItemIndexDataPage<int>;
            page.RemoveEntry(e => e.Id == 1);
            pageManager.SavePage(page);

            // Verify
            Assert.AreEqual(DataPageType.Items, page.Header.DataPageType);
            Assert.IsTrue((new[] { 0, 2 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 1 }).SequenceEqual(pageManager.ItemDataPages));
            VerifyPersistedData(ioManager.GetPageData(1), Data.ItemPage(null, null, 0, 0, 0, Data.PageHeaderSize));
        }

        /// <summary>
        /// The last index node page should not be invalidated - it should remain in the page manager.
        /// </summary>
        [TestMethod]
        public void ShouldLeaveTheLastIndexNodePageWhenInvalidated()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, null, null)
                                .ItemPage<int>(1, null, 2, null)
                                .ItemPage<int>(2, 1, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(0);
            pageManager.SavePage(page);

            // Verify
            Assert.AreEqual(DataPageType.IndexNode, page.Header.DataPageType);
            Assert.IsTrue((new[] { 0 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 1, 2 }).SequenceEqual(pageManager.ItemDataPages));
            Assert.AreEqual(0, pageManager.UnusedPageCount);
        }

        /// <summary>
        /// The page manager page list and page link references should be update when the first index node page is invalidated.
        /// </summary>
        [TestMethod]
        public void ShouldUpdateRelevantReferencesWhenFirstIndexNodePageInvalidated()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, null)
                                .ItemPage<int>(1, null, null, null)
                                .IndexNodePage(2, 0, 3, null)
                                .IndexNodePage(3, 2, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(0);
            pageManager.SavePage(page);

            // Verify
            Assert.AreEqual(DataPageType.Unused, page.Header.DataPageType);
            Assert.IsNull(page.Header.PreviousPage);
            Assert.IsNull(page.Header.NextPage);
            AssertPageReferences(pageManager.GetPage(1), 1, null, null);
            AssertPageReferences(pageManager.GetPage(2), 2, null, 3);
            AssertPageReferences(pageManager.GetPage(3), 3, 2, null);
            Assert.IsTrue((new[] { 2, 3 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 1 }).SequenceEqual(pageManager.ItemDataPages));
            Assert.AreEqual(1, pageManager.UnusedPageCount);
        }

        /// <summary>
        /// The page manager should flush the updated page references to the underlying IO manager.
        /// </summary>
        [TestMethod]
        public void ShouldFlushTheUpdatedLinkReferencesToTheIOManagerWhenInvalidatingFirstPage()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, null)
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 3))
                                .IndexNodePage(2, 0, 3, new ItemRefSetup(2, 5, 8).AndNodeRef(6, 4, 'a'))
                                .IndexNodePage(3, 2, null, new ItemRefSetup(8, 3, 9))
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(0);
            pageManager.SavePage(page);

            // Verify
            VerifyPersistedData(ioManager.PageManagerHeader, Data.Start.Then(1, 2, 4));
            VerifyPersistedData(ioManager.GetPageData(0), EmptyPage);
            VerifyPersistedData(ioManager.GetPageData(1), Data.ItemPage(null, null, 1, 1, 1, Data.PageHeaderSize + 8).IndexedItem(1, 3));
            VerifyPersistedData(ioManager.GetPageData(2), Data.IndexNodePage(null, 3, 2, 2, 6, Data.PageHeaderSize + 13 + 10).ItemReference(2, 5, 8).NodeReference(6, 4, 'a'));
            VerifyPersistedData(ioManager.GetPageData(3), Data.IndexNodePage(2, null, 1, 8, 8, Data.PageHeaderSize + 13).ItemReference(8, 3, 9));
        }

        /// <summary>
        /// The page manager page list and page link references should be update when a middle index node page is invalidated.
        /// </summary>
        [TestMethod]
        public void ShouldUpdateRelevantReferencesWhenMiddleIndexNodePageInvalidated()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, null)
                                .ItemPage<int>(1, null, null, null)
                                .IndexNodePage(2, 0, 3, null)
                                .IndexNodePage(3, 2, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(2);
            pageManager.SavePage(page);

            // Verify
            Assert.AreEqual(DataPageType.Unused, page.Header.DataPageType);
            Assert.IsNull(page.Header.PreviousPage);
            Assert.IsNull(page.Header.NextPage);
            AssertPageReferences(pageManager.GetPage(0), 0, null, 3);
            AssertPageReferences(pageManager.GetPage(1), 1, null, null);
            AssertPageReferences(pageManager.GetPage(3), 3, 0, null);
            Assert.IsTrue((new[] { 0, 3 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 1 }).SequenceEqual(pageManager.ItemDataPages));
            Assert.AreEqual(1, pageManager.UnusedPageCount);
        }

        /// <summary>
        /// The page manager should flush the updated page references to the underlying IO manager.
        /// </summary>
        [TestMethod]
        public void ShouldFlushTheUpdatedLinkReferencesToTheIOManagerWhenInvalidatingMiddlePage()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, new ItemRefSetup(2, 5, 99).AndNodeRef(6, 4, 'a'))
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 3))
                                .IndexNodePage(2, 0, 3, null)
                                .IndexNodePage(3, 2, null, new ItemRefSetup(8, 3, 0))
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(2);
            pageManager.SavePage(page);

            // Verify
            VerifyPersistedData(ioManager.PageManagerHeader, Data.Start.Then(1, 0, 4));
            VerifyPersistedData(ioManager.GetPageData(0), Data.IndexNodePage(null, 3, 2, 2, 6, Data.PageHeaderSize + 13 + 10).ItemReference(2, 5, 99).NodeReference(6, 4, 'a'));
            VerifyPersistedData(ioManager.GetPageData(1), Data.ItemPage(null, null, 1, 1, 1, Data.PageHeaderSize + 8).IndexedItem(1, 3));
            VerifyPersistedData(ioManager.GetPageData(2), EmptyPage);
            VerifyPersistedData(ioManager.GetPageData(3), Data.IndexNodePage(0, null, 1, 8, 8, Data.PageHeaderSize + 13).ItemReference(8, 3, 0));
        }

        /// <summary>
        /// The page manager page list and page link references should be update when the last index node page is invalidated.
        /// </summary>
        [TestMethod]
        public void ShouldUpdateRelevantReferencesWhenLastIndexNodePageInvalidated()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, null)
                                .ItemPage<int>(1, null, null, null)
                                .IndexNodePage(2, 0, 3, null)
                                .IndexNodePage(3, 2, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(3);
            pageManager.SavePage(page);

            // Verify
            Assert.AreEqual(DataPageType.Unused, page.Header.DataPageType);
            Assert.IsNull(page.Header.PreviousPage);
            Assert.IsNull(page.Header.NextPage);
            AssertPageReferences(pageManager.GetPage(0), 0, null, 2);
            AssertPageReferences(pageManager.GetPage(1), 1, null, null);
            AssertPageReferences(pageManager.GetPage(2), 2, 0, null);
            Assert.IsTrue((new[] { 0, 2 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 1 }).SequenceEqual(pageManager.ItemDataPages));
            Assert.AreEqual(1, pageManager.UnusedPageCount);
        }

        /// <summary>
        /// The page manager should flush the updated page references to the underlying IO manager.
        /// </summary>
        [TestMethod]
        public void ShouldFlushTheUpdatedLinkReferencesToTheIOManagerWhenInvalidatingLastPage()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, new ItemRefSetup(2, 5, 88).AndNodeRef(6, 4, 'a'))
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 3))
                                .IndexNodePage(2, 0, 3, new ItemRefSetup(8, 3, 12))
                                .IndexNodePage(3, 2, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = pageManager.GetPage(3);
            pageManager.SavePage(page);

            // Verify
            VerifyPersistedData(ioManager.PageManagerHeader, Data.Start.Then(1, 0, 4));
            VerifyPersistedData(ioManager.GetPageData(0), Data.IndexNodePage(null, 2, 2, 2, 6, Data.PageHeaderSize + 13 + 10).ItemReference(2, 5, 88).NodeReference(6, 4, 'a'));
            VerifyPersistedData(ioManager.GetPageData(1), Data.ItemPage(null, null, 1, 1, 1, Data.PageHeaderSize + 8).IndexedItem(1, 3));
            VerifyPersistedData(ioManager.GetPageData(2), Data.IndexNodePage(0, null, 1, 8, 8, Data.PageHeaderSize + 13).ItemReference(8, 3, 12));
            VerifyPersistedData(ioManager.GetPageData(3), EmptyPage);
        }
    }
}

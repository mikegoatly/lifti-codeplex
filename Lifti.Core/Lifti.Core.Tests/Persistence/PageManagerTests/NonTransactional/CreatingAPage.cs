// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.NonTransactional
{
    using System;
    using System.Collections.Generic;

    using Lifti.Extensibility;
    using Lifti.Persistence;
    using Lifti.Tests.Persistence.PageManagerTests.Setup;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the page creation process of the page manager.
    /// </summary>
    [TestFixture]
    public class CreatingAPage : PageManagerTestBase
    {
        /// <summary>
        /// When creating a new item data page, a page manager should prefer to use an existing empty page over extending the data.
        /// </summary>
        [Test]
        public void ShouldAllocateNewItemDataPageFromExistingEmptyPageIfAvailable()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                .IndexNodePage(0, null, null, null)
                .ItemPage<int>(1, null, null, null)
                .EmptyPage(2)
                .Prepare();

            var persistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var pageManager = CreatePageManager(settings, ioManager, persistence.Object);

            // Execute
            var page = (ItemIndexDataPage<int>)pageManager.GetPage(1);
            var newPage = pageManager.CreatePage(page);

            // Verify
            Assert.IsInstanceOf<ItemIndexDataPage<int>>(newPage);
            AssertPageReferences(page, 1, null, 2);
            AssertPageReferences(newPage, 2, 1, null);

            // Verify the updated references were stored
            VerifyPersistedData(ioManager.PageManagerHeader, Data.Start.Then(1, 0, 3, 4));
            VerifyPersistedData(ioManager.GetPageData(0), Data.IndexNodePage(null, null, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(1), Data.ItemPage(null, 2, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(2), Data.ItemPage(1, null, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(3), Data.ItemNodeIndexPage(null, null, 0, 0, 0, Data.PageHeaderSize));

            Assert.AreEqual(0, pageManager.UnusedPageCount);
            Assert.AreEqual(4, pageManager.TotalPageCount);
            Assert.IsTrue((new[] { 1, 2 }).SequenceEqual(pageManager.ItemDataPages));
            Assert.IsTrue((new[] { 0 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 3 }).SequenceEqual(pageManager.ItemNodeIndexDataPages));

            settings.VerifyAll();
            ioManager.VerifyBasics();
            persistence.VerifyAll();
        }

        /// <summary>
        /// When creating a new item data page, a page manager should extend the data if required.
        /// </summary>
        [Test]
        public void ShouldExtendDataIfNoRoomAvailableWhenCreatingNewItemDataPage()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            settings.SetupGet(s => s.GrowPageCount).Returns(4);
            var ioManager = new MockDataFileManagerSetup(false)
                .IndexNodePage(0, null, null, null)
                .ItemPage<int>(1, null, null, null)
                .EmptyPage(2)
                .Prepare();

            var persistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var pageManager = CreatePageManager(settings, ioManager, persistence.Object);

            // Execute
            var page = (ItemIndexDataPage<int>)pageManager.GetPage(1);

            // Order will be 1, 2 after this - newPage1 is 2
            var newPage1 = pageManager.CreatePage(page);

            // Order will be 1, 4, 2 after this - newPage2 is 4
            var newPage2 = pageManager.CreatePage(page);

            // Order will be 1, 4, 2, 5 after this - newPage3 is 5
            var newPage3 = pageManager.CreatePage(newPage1);

            // Verify
            Assert.IsInstanceOf<ItemIndexDataPage<int>>(newPage1);
            Assert.IsInstanceOf<ItemIndexDataPage<int>>(newPage2);
            AssertPageReferences(page, 1, null, 4);
            AssertPageReferences(newPage2, 4, 1, 2);
            AssertPageReferences(newPage1, 2, 4, 5);
            AssertPageReferences(newPage3, 5, 2, null);

            // Verify the updated references were stored
            VerifyPersistedData(ioManager.PageManagerHeader, Data.Start.Then(1, 0, 3, 8));
            VerifyPersistedData(ioManager.GetPageData(0), Data.IndexNodePage(null, null, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(1), Data.ItemPage(null, 4, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(2), Data.ItemPage(4, 5, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(3), Data.ItemNodeIndexPage(null, null, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(4), Data.ItemPage(1, 2, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(5), Data.ItemPage(2, null, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(6), EmptyPage);
            VerifyPersistedData(ioManager.GetPageData(7), EmptyPage);

            Assert.AreEqual(2, pageManager.UnusedPageCount);
            Assert.AreEqual(8, pageManager.TotalPageCount);
            Assert.IsTrue((new[] { 1, 4, 2, 5 }).SequenceEqual(pageManager.ItemDataPages));
            Assert.IsTrue((new[] { 0 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 3 }).SequenceEqual(pageManager.ItemNodeIndexDataPages));

            settings.VerifyAll();
            ioManager.VerifyBasics();
            persistence.VerifyAll();
        }

        /// <summary>
        /// When creating a new node data page, a page manager should prefer to use an existing empty page over extending the data.
        /// </summary>
        [Test]
        public void ShouldAllocateNewNodeDataPageFromExistingEmptyPageIfAvailable()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                .IndexNodePage(0, null, null, null)
                .ItemPage<int>(1, null, null, null)
                .EmptyPage(2)
                .Prepare();

            var persistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var pageManager = CreatePageManager(settings, ioManager, persistence.Object);

            // Execute
            var page = (IndexNodeDataPage)pageManager.GetPage(0);
            var newPage = pageManager.CreatePage(page);

            // Verify
            Assert.IsInstanceOf<IndexNodeDataPage>(newPage);
            AssertPageReferences(page, 0, null, 2);
            AssertPageReferences(newPage, 2, 0, null);

            // Verify the updated references were stored
            VerifyPersistedData(ioManager.PageManagerHeader, Data.Start.Then(1, 0, 3, 4));
            VerifyPersistedData(ioManager.GetPageData(0), Data.IndexNodePage(null, 2, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(1), Data.ItemPage(null, null, 0, 0, 0, Data.PageHeaderSize));
            VerifyPersistedData(ioManager.GetPageData(2), Data.IndexNodePage(0, null, 0, 0, 0, Data.PageHeaderSize));

            Assert.AreEqual(0, pageManager.UnusedPageCount);
            Assert.AreEqual(4, pageManager.TotalPageCount);
            Assert.IsTrue((new[] { 1 }).SequenceEqual(pageManager.ItemDataPages));
            Assert.IsTrue((new[] { 0, 2 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 3 }).SequenceEqual(pageManager.ItemNodeIndexDataPages));

            settings.VerifyAll();
            ioManager.VerifyBasics();
            persistence.VerifyAll();
        }

        /// <summary>
        /// Creating a new page should cause the new page to be cached.
        /// </summary>
        [Test]
        public void ShouldCacheTheNewPage()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false).IndexNodePage(0, null, null, null).ItemPage<int>(1, null, null, null).EmptyPage(2).Prepare();
            var persistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);

            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();
            var cache = new Mock<IPageCache>();
            var headerCache = new Dictionary<int, IDataPageHeader>();
            cache.Setup(c => c.GetCachedPage(It.IsAny<IDataPageHeader>(), It.IsAny<Func<IDataPageHeader, IDataPage>>())).Returns<IDataPageHeader, Func<IDataPageHeader, IDataPage>>((h, l) => l(h));
            cache.Setup(c => c.GetHeader(It.IsAny<int>(), It.IsAny<Func<int, IDataPageHeader>>())).Returns<int, Func<int, IDataPageHeader>>((i, l) =>
                {
                    if (!headerCache.ContainsKey(i))
                    {
                        headerCache.Add(i, l(i));
                    }

                    return headerCache[i];
                });

            cache.Setup(c => c.GetHeader(It.IsAny<int>())).Returns<int>(i => headerCache[i]);

            var pageManager = new PageManager<int>(cache.Object, settings.Object, ioManager.Mock.Object, persistence.Object, extensibilityService.Object);
            pageManager.Initialize();

            // Execute
            var page = (IndexNodeDataPage)pageManager.GetPage(0);
            var newPage = pageManager.CreatePage(page);

            cache.Verify(c => c.CachePage(newPage));
        }
    }
}

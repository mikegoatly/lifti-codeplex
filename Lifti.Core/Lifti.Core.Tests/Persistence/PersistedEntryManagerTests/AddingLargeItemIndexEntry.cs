// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    /// <summary>
    /// Tests for the adding large entries to a page.
    /// </summary>
    [TestFixture]
    public class AddingLargeItemIndexEntry : PersistedEntryManagerTestBase
    {
        /// <summary>
        /// If an item is added to a page and it can't fit, a page split should occur.
        /// </summary>
        [Test]
        public void ShouldCausePageSplitIfEntryTooLargeForRemainingPageSpace()
        {
            var pageManager = CreateMockedPageManager();
            var headers = new[]
                {
                    new DataPageHeader(DataPageType.Items, 0, null, null, 1, 0, 0, Data.PageHeaderSize + 3002)
                };

            var page0Entries = new[] 
                {
                    new ItemEntry<string>(0, new string('a', 3000), 3002)
                };

            var page0 = new ItemIndexDataPage<string>(headers[0], page0Entries);
            pageManager.Setup(p => p.GetPage(headers[0])).Returns(page0);
            pageManager.SetupGet(p => p.ItemDataPages).Returns(new DataPageCollection(headers));

            var page1 = new ItemIndexDataPage<string>(new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 0, 0, 0, Data.PageHeaderSize), new ItemEntry<string>[0]);
            pageManager.Setup(m => m.CreatePage((DataPage<ItemEntry<string>>)page0)).Returns(page1);
            pageManager.Setup(m => m.SavePage(page1));

            var typePersistence = new StringPersistence();

            var manager = new PersistedEntryManager<string>(pageManager.Object, typePersistence);
            manager.Initialize();

            var bigString = new string('b', 6500);
            manager.AddItemIndexEntry(1, bigString);

            Assert.AreEqual(1, page0.Entries.Count());
            Assert.AreEqual(1, page1.Entries.Count());

            Assert.AreEqual(bigString, page1.Entries.First().Item);
            Assert.AreEqual(1, page1.Entries.First().Id);
            Assert.AreEqual(6506, page1.Entries.First().Size);
        }

        /// <summary>
        /// If an item is added to a page and it can't fit, a page split should occur. If the item then cannot fit on either
        /// page, another page split should occur.
        /// </summary>
        [Test]
        public void ShouldCauseTwoPageSplitsIfEntryCausesAPageSplitAndCantFitOnEither()
        {
            var pageManager = CreateMockedPageManager();
            var headers = new[]
                {
                    new DataPageHeader(DataPageType.Items, 0, null, null, 2, 0, 2, Data.PageHeaderSize + 7004)
                };

            var page0Entries = new[] 
                {
                    new ItemEntry<string>(0, new string('a', 4000), 4002),
                    new ItemEntry<string>(2, new string('c', 3000), 3002)
                };

            var page0 = new ItemIndexDataPage<string>(headers[0], page0Entries);
            pageManager.Setup(p => p.GetPage(headers[0])).Returns(page0);
            pageManager.SetupGet(p => p.ItemDataPages).Returns(new DataPageCollection(headers));
            pageManager.Setup(m => m.SavePage(page0));

            var page1 = new ItemIndexDataPage<string>(new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 0, 0, 0, Data.PageHeaderSize), new ItemEntry<string>[0]);
            pageManager.Setup(m => m.SavePage(page1));
            var page2 = new ItemIndexDataPage<string>(new DataPageHeader(DataPageType.IndexNode, 2, 0, null, 0, 0, 0, Data.PageHeaderSize), new ItemEntry<string>[0]);
            pageManager.Setup(m => m.SavePage(page2));

            pageManager.Setup(m => m.CreatePage((DataPage<ItemEntry<string>>)page0)).ReturnsInOrder(page1, page2);

            var typePersistence = new StringPersistence();

            var manager = new PersistedEntryManager<string>(pageManager.Object, typePersistence);
            manager.Initialize();

            var bigString = new string('b', 6500);
            manager.AddItemIndexEntry(1, bigString);

            Assert.AreEqual(1, page0.Entries.Count());
            Assert.AreEqual(1, page1.Entries.Count());
            Assert.AreEqual(1, page2.Entries.Count());

            Assert.AreEqual(0, page0.Entries.First().Id);

            Assert.AreEqual(bigString, page2.Entries.First().Item);
            Assert.AreEqual(1, page2.Entries.First().Id);
            Assert.AreEqual(6506, page2.Entries.First().Size);

            Assert.AreEqual(2, page1.Entries.First().Id);
        }
    }
}

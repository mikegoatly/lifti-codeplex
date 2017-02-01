// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using System.Linq;

    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for adding item index entries.
    /// </summary>
    [TestClass]
    public class AddingItemIndexEntry : PersistedEntryManagerTestBase
    {
        /// <summary>
        /// When an item entry is inserted and it fits into an existing page, it should be inserted into the correct location.
        /// </summary>
        [TestMethod]
        public void NodeEntryShouldBeInsertedIntoMidPageWhenItFits()
        {
            var pageManager = CreateMockedPageManager();
            var headers = new[]
                {
                    new DataPageHeader(DataPageType.Items, 0, null, 1, 2, 0, 1, Data.PageHeaderSize + 20),
                    new DataPageHeader(DataPageType.Items, 1, 0, 2, 3, 2, 4, Data.PageHeaderSize + 30),
                    new DataPageHeader(DataPageType.Items, 2, 1, null, 2, 5, 9, Data.PageHeaderSize + 20)
                };

            var page0Entries = new[] 
                {
                    new ItemEntry<int>(0, 3, 10),
                    new ItemEntry<int>(1, 12, 10)
                };

            var page1Entries = new[]
                {
                    new ItemEntry<int>(2, 1, 10),
                    new ItemEntry<int>(3, 11, 10),
                    new ItemEntry<int>(4, 7, 10)
                };

            var page2Entries = new[]
                {
                    new ItemEntry<int>(5, 8, 10),
                    new ItemEntry<int>(9, 4, 10)
                };

            var page0 = new ItemIndexDataPage<int>(headers[0], page0Entries);
            var page1 = new ItemIndexDataPage<int>(headers[1], page1Entries);
            var page2 = new ItemIndexDataPage<int>(headers[2], page2Entries);

            pageManager.Setup(p => p.GetPage(headers[0])).Returns(page0);
            pageManager.Setup(p => p.GetPage(headers[1])).Returns(page1);
            pageManager.Setup(p => p.GetPage(headers[2])).Returns(page2);

            pageManager.SetupGet(p => p.ItemDataPages).Returns(new DataPageCollection(headers));

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            typePersistence.SetupGet(p => p.SizeReader).Returns(i => 6);

            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);
            manager.Initialize();

            pageManager.Setup(m => m.SavePage(page2));

            manager.AddItemIndexEntry(7, 99);

            Assert.AreEqual(3, page2.Entries.Count());

            Assert.IsTrue((new[] { 5, 7, 9 }).SequenceEqual(page2.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 8, 99, 4 }).SequenceEqual(page2.Entries.Select(e => e.Item)));
            Assert.IsTrue((new short[] { 10, 10, 10 }).SequenceEqual(page2.Entries.Select(e => e.Size)));

            pageManager.Verify(m => m.SavePage(page0), Times.Never());
            pageManager.Verify(m => m.SavePage(page1), Times.Never());
            pageManager.Verify(m => m.SavePage(page2), Times.Exactly(1));
        }
    }
}

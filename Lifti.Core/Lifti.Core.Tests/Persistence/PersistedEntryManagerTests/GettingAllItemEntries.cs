// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using System.Linq;

    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for getting all item entries from the <see cref="PersistedEntryManager{TKey}"/> class.
    /// </summary>
    [TestClass]
    public class GettingAllItemEntries : PersistedEntryManagerTestBase
    {
        /// <summary>
        /// If the index contains no items, an empty enumerable should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldYieldNoResultsForEmptyIndex()
        {
            var pageManager = CreateMockedPageManager();
            var headers = new[]
            {
                new DataPageHeader(DataPageType.Items, 0, null, null, 0, 0, 0, Data.PageHeaderSize)
            };

            pageManager.SetupGet(p => p.ItemDataPages).Returns(new DataPageCollection(headers));
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            Assert.AreEqual(0, manager.GetAllItemEntries().Count());
        }

        /// <summary>
        /// If the index contains items, they should be returned in the order in which they are stored.
        /// </summary>
        [TestMethod]
        public void ShouldYieldResultsFromAllPagesInOrder()
        {
            var pageManager = CreateMockedPageManager();
            var headers = new[]
            {
                new DataPageHeader(DataPageType.Items, 0, null, 2, 2, 2, 4, Data.PageHeaderSize + 16),
                new DataPageHeader(DataPageType.Items, 2, 0, 1, 3, 5, 7, Data.PageHeaderSize + 24),
                new DataPageHeader(DataPageType.Items, 1, 2, null, 1, 9, 9, Data.PageHeaderSize + 8)
            };

            pageManager.Setup(p => p.GetPage(headers[0])).Returns(new ItemIndexDataPage<int>(headers[0], new[] { new ItemEntry<int>(2, 55, 8), new ItemEntry<int>(4, 66, 8) }));
            pageManager.Setup(p => p.GetPage(headers[2])).Returns(new ItemIndexDataPage<int>(headers[2], new[] { new ItemEntry<int>(9, 77, 8) }));
            pageManager.Setup(p => p.GetPage(headers[1])).Returns(new ItemIndexDataPage<int>(headers[1], new[] { new ItemEntry<int>(5, 33, 8), new ItemEntry<int>(6, 44, 8), new ItemEntry<int>(7, 99, 8) }));

            pageManager.SetupGet(p => p.ItemDataPages).Returns(new DataPageCollection(headers));
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            Assert.IsTrue((new[] { 55, 66, 33, 44, 99, 77 }).SequenceEqual(manager.GetAllItemEntries().Select(i => i.Item)));
        }
    }
}

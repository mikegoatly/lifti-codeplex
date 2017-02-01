// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageTests
{
    using System;
    using System.Linq;

    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for moving entries between pages.
    /// </summary>
    [TestClass]
    public class MovingEntries : UnitTestBase
    {
        /// <summary>
        /// When moving entries, any that match the given predicate should be moved to the page.
        /// </summary>
        [TestMethod]
        public void ShouldMoveAllEntriesThatMatchPredicate()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, 1, 4, 2, 4, Data.PageHeaderSize + 13 + 13 + 10 + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new NodeReferenceIndexNodeEntry(3, 9, 'b'), new ItemReferenceIndexNodeEntry(3, 2, 9), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            var newPage = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 0, 0, 0, Data.PageHeaderSize),
                new IndexNodeEntryBase[0]);

            newPage.MoveEntriesFrom(page, n => n.Id >= 3);

            Assert.IsTrue((new[] { 2 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));
            Assert.IsTrue((new[] { 3, 3, 4 }).SequenceEqual(newPage.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 9, 2, 2 }).SequenceEqual(newPage.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(1, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 10, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(2, page.Header.LastEntry);

            Assert.AreEqual(3, newPage.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 10 + 13 + 13, newPage.Header.CurrentSize);
            Assert.AreEqual(3, newPage.Header.FirstEntry);
            Assert.AreEqual(4, newPage.Header.LastEntry);
        }

        /// <summary>
        /// If a null predicate is provided, an argument null exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfNullPredicateProvided()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 1, 2, 2, Data.PageHeaderSize + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a') });

            var newPage = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 1, 0, null, 0, 0, 0, Data.PageHeaderSize),
                new IndexNodeEntryBase[0]);

            AssertRaisesArgumentNullException(() => newPage.MoveEntriesFrom(page, null), "predicate");
        }

        /// <summary>
        /// If a null page is provided, an argument null exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfNullFromPageProvided()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 1, 2, 2, Data.PageHeaderSize + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a') });

            AssertRaisesArgumentNullException(() => page.MoveEntriesFrom(null, e => e.Id == 1), "from");
        }

        /// <summary>
        /// If an attempt to move entries to the same page, an argument exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfPagesAreSame()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 1, 2, 2, Data.PageHeaderSize + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a') });

            AssertRaisesException<ArgumentException>(() => page.MoveEntriesFrom(page, e => e.Id == 1), "The page being copied from must be different to the current page\r\nParameter name: from");
        }
    }
}

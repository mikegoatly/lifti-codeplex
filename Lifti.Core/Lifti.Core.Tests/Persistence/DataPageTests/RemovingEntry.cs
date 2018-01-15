// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageTests
{
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    /// <summary>
    /// Tests for removing entries from a data page.
    /// </summary>
    [TestFixture]
    public class RemovingEntry
    {
        /// <summary>
        /// When requested an entry should be removed from the front of the list.
        /// </summary>
        [Test]
        public void ShouldRemoveEntryFromFrontOfList()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 3, 2, 4, Data.PageHeaderSize + 13 + 10 + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new NodeReferenceIndexNodeEntry(3, 9, 'b'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            Assert.IsTrue(page.RemoveEntry(e => e.Id == 2));

            Assert.IsTrue((new[] { 3, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 9, 2 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(2, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 13 + 10, page.Header.CurrentSize);
            Assert.AreEqual(3, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }

        /// <summary>
        /// When requested an entry should be removed from the end of the list.
        /// </summary>
        [Test]
        public void ShouldRemoveEntryFromEndOfList()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 3, 2, 4, Data.PageHeaderSize + 13 + 10 + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new NodeReferenceIndexNodeEntry(3, 9, 'b'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            Assert.IsTrue(page.RemoveEntry(e => e.Id == 4));

            Assert.IsTrue((new[] { 2, 3 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 9 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(2, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 10 + 10, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(3, page.Header.LastEntry);
        }

        /// <summary>
        /// When requested an entry should be removed from the middle of the list.
        /// </summary>
        [Test]
        public void ShouldRemoveEntryFromMiddleOfList()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 3, 2, 4, Data.PageHeaderSize + 13 + 10 + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new NodeReferenceIndexNodeEntry(3, 9, 'b'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            Assert.IsTrue(page.RemoveEntry(e => e.Id == 3));

            Assert.IsTrue((new[] { 2, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 2 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(2, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 10 + 13, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }

        /// <summary>
        /// If the predicate matches multiple entries, then all of them should be removed.
        /// </summary>
        [Test]
        public void ShouldRemoveMultipleEntriesIfAppropriate()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 4, 2, 4, Data.PageHeaderSize + 13 + 13 + 10 + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new NodeReferenceIndexNodeEntry(3, 9, 'b'), new ItemReferenceIndexNodeEntry(3, 2, 9), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            Assert.IsTrue(page.RemoveEntry(e => e.Id == 3));

            Assert.IsTrue((new[] { 2, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 2 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(2, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 10 + 13, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }

        /// <summary>
        /// If no items match the predicate, then the method call should return false.
        /// </summary>
        [Test]
        public void ShouldReturnFalseIfNoItemsDeleted()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 3, 2, 4, Data.PageHeaderSize + 13 + 10 + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new NodeReferenceIndexNodeEntry(3, 9, 'b'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            Assert.IsFalse(page.RemoveEntry(e => e.Id == 1));

            Assert.IsTrue((new[] { 2, 3, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 9, 2 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(3, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 13 + 10 + 10, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }

        /// <summary>
        /// Removing the last entry from the page should mark the first and last entry ids as zero.
        /// </summary>
        [Test]
        public void RemovingLastEntryFromPageShouldMarkTheFirstAndLastEntryIdsAsZero()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 1, 2, 2, Data.PageHeaderSize + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a') });

            Assert.IsTrue(page.RemoveEntry(e => e.Id == 2));

            Assert.AreEqual(0, page.Entries.Count());

            Assert.AreEqual(0, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize, page.Header.CurrentSize);
            Assert.AreEqual(0, page.Header.FirstEntry);
            Assert.AreEqual(0, page.Header.LastEntry);
        }

        /// <summary>
        /// If a null predicate is provided, an argument null exception should be thrown.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfNullPredicateProvided()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 1, 2, 2, Data.PageHeaderSize + 10),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a') });

            this.AssertRaisesArgumentNullException(() => page.RemoveEntry(null), "predicate");
        }
    }
}

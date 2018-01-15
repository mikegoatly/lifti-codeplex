// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageTests
{
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    /// <summary>
    /// Tests for adding entries to data pages.
    /// </summary>
    [TestFixture]
    public class AddingEntry
    {
        /// <summary>
        /// A persistence exception should be thrown if the added item cannot be fitted onto a page.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfAddedItemCannotFitOntoPage()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 2, 2, 4, Data.PageSize - 12),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            Assert.Throws<PersistenceException>(
                () => page.AddEntry(new ItemReferenceIndexNodeEntry(1, 7, 22)),
                "Added entry will not fit into the page.");
        }

        /// <summary>
        /// A new item should be inserted if it exactly fits into the remaining space in the page.
        /// </summary>
        [Test]
        public void ShouldAddItemIfItMeansThePageWillBeCompletelyFull()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 2, 2, 4, Data.PageSize - 13),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            page.AddEntry(new ItemReferenceIndexNodeEntry(1, 7, 22));

            Assert.IsTrue((new[] { 1, 2, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 7, 5, 2 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(3, page.Header.EntryCount);
            Assert.AreEqual(Data.PageSize, page.Header.CurrentSize);
            Assert.AreEqual(1, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }

        /// <summary>
        /// A new item should be inserted as the first entry in a page if its id is less than the id of the first 
        /// entry in the page.
        /// </summary>
        [Test]
        public void ShouldInsertEntryAtStartOfPageIfLessThanFirstEntry()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 2, 2, 4, Data.PageHeaderSize + 10 + 13),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            page.AddEntry(new ItemReferenceIndexNodeEntry(1, 7, 22));

            Assert.IsTrue((new[] { 1, 2, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 7, 5, 2 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(3, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 36, page.Header.CurrentSize);
            Assert.AreEqual(1, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }

        /// <summary>
        /// A new item should be inserted as the first entry in a page if its id is equal to the id of the first 
        /// entry in the page.
        /// </summary>
        [Test]
        public void ShouldInsertEntryAtStartOfPageIfEqualToFirstEntry()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 2, 2, 4, Data.PageHeaderSize + 10 + 13),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            page.AddEntry(new ItemReferenceIndexNodeEntry(2, 7, 22));

            Assert.IsTrue((new[] { 2, 2, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 7, 5, 2 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(3, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 36, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }

        /// <summary>
        /// A new item should be inserted as the last entry in a page if its id is greater than the id of the last 
        /// entry in the page.
        /// </summary>
        [Test]
        public void ShouldInsertEntryAtEndOfPageIfGreaterThanLastEntry()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 2, 2, 4, Data.PageHeaderSize + 10 + 13),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            page.AddEntry(new ItemReferenceIndexNodeEntry(5, 7, 22));

            Assert.IsTrue((new[] { 2, 4, 5 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 2, 7 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(3, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 36, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(5, page.Header.LastEntry);
        }

        /// <summary>
        /// A new item should be inserted as the last entry in a page if its id is equal to the id of the last 
        /// entry in the page.
        /// </summary>
        [Test]
        public void ShouldInsertEntryAtEndOfPageIfEqualToLastEntry()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 2, 2, 4, Data.PageHeaderSize + 10 + 13),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            page.AddEntry(new ItemReferenceIndexNodeEntry(4, 7, 22));

            Assert.IsTrue((new[] { 2, 4, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 2, 7 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(3, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 36, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }

        /// <summary>
        /// A new item should be inserted at the correct location in the page if its id is between the ids of the 
        /// start and end entries in the page.
        /// </summary>
        [Test]
        public void ShouldInsertEntryAtCorrectLocationIfIdIsBetweenStartAndEndEntries()
        {
            var page = new IndexNodeDataPage(
                new DataPageHeader(DataPageType.IndexNode, 0, null, null, 2, 2, 4, Data.PageHeaderSize + 10 + 13),
                new IndexNodeEntryBase[] { new NodeReferenceIndexNodeEntry(2, 5, 'a'), new ItemReferenceIndexNodeEntry(4, 2, 9) });

            page.AddEntry(new ItemReferenceIndexNodeEntry(3, 7, 22));

            Assert.IsTrue((new[] { 2, 3, 4 }).SequenceEqual(page.Entries.Select(e => e.Id)));
            Assert.IsTrue((new[] { 5, 7, 2 }).SequenceEqual(page.Entries.Select(e => e.ReferencedId)));

            Assert.AreEqual(3, page.Header.EntryCount);
            Assert.AreEqual(Data.PageHeaderSize + 36, page.Header.CurrentSize);
            Assert.AreEqual(2, page.Header.FirstEntry);
            Assert.AreEqual(4, page.Header.LastEntry);
        }
    }
}

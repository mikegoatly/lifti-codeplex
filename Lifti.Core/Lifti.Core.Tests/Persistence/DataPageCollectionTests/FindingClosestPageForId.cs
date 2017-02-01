// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageCollectionTests
{
    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the FindClosestPageForEntry method of the DataPageCollection.
    /// </summary>
    [TestClass]
    public class FindingClosestPageForId
    {
        /// <summary>
        /// When the collection is empty, requesting the closest page to any id should return null.
        /// </summary>
        [TestMethod]
        public void ShouldReturnNullIfCollectionIsEmpty()
        {
            var col = new DataPageCollection();

            Assert.IsNull(col.FindClosestPageForEntry(0));
        }

        /// <summary>
        /// When the collection has only one page, it will always be returned regardless of the
        /// requested id.
        /// </summary>
        [TestMethod]
        public void ShouldAlwaysReturnPageIfCollectionHasOnlyOnePage()
        {
            var headers = new[] { new DataPageHeader(DataPageType.Items, 0, null, null, 4, 1, 3, 30) };
            var col = new DataPageCollection(headers);

            Assert.AreSame(headers[0], col.FindClosestPageForEntry(0));
            Assert.AreSame(headers[0], col.FindClosestPageForEntry(1));
            Assert.AreSame(headers[0], col.FindClosestPageForEntry(2));
            Assert.AreSame(headers[0], col.FindClosestPageForEntry(3));
            Assert.AreSame(headers[0], col.FindClosestPageForEntry(4));
        }

        /// <summary>
        /// If the requested ID falls between two pages, then the first one should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldReturnFirstPageIfRequestedIdFallsBetweenTwoPages()
        {
            var headers = new[] 
            { 
                new DataPageHeader(DataPageType.Items, 0, null, 1, 4, 1, 3, 30),
                new DataPageHeader(DataPageType.Items, 1, 0, 2, 4, 7, 9, 30),
                new DataPageHeader(DataPageType.Items, 2, 1, null, 4, 13, 16, 30)
            };

            var col = new DataPageCollection(headers);
            Assert.AreSame(headers[0], col.FindClosestPageForEntry(4));
            Assert.AreSame(headers[0], col.FindClosestPageForEntry(5));
            Assert.AreSame(headers[0], col.FindClosestPageForEntry(6));

            Assert.AreSame(headers[1], col.FindClosestPageForEntry(10));
            Assert.AreSame(headers[1], col.FindClosestPageForEntry(11));
            Assert.AreSame(headers[1], col.FindClosestPageForEntry(12));
        }
    }
}

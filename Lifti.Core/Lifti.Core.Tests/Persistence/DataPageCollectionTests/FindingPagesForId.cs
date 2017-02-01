// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageCollectionTests
{
    using System.Linq;

    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for finding pages by id.
    /// </summary>
    [TestClass]
    public class FindingPagesForId
    {
        /// <summary>
        /// The FindPagesForEntry method should yield no results if the collection is empty.
        /// </summary>
        [TestMethod]
        public void ShouldYieldNoResultsIfCollectionEmpty()
        {
            var col = new DataPageCollection();

            Assert.AreEqual(0, col.FindPagesForEntry(7).Count());
            Assert.AreEqual(0, col.FindPagesForEntry(0).Count());
        }

        /// <summary>
        /// The FindPagesForEntry method should yield no results if the collection doesn't contain entries for the 
        /// requested id.
        /// </summary>
        [TestMethod]
        public void ShouldYieldNoResultsIfCollectionHasOnePageItDoesntContainId()
        {
            var col = new DataPageCollection(new[] { new DataPageHeader(DataPageType.Items, 0, null, null, 4, 1, 3, 30) });

            Assert.AreEqual(0, col.FindPagesForEntry(4).Count());
            Assert.AreEqual(0, col.FindPagesForEntry(0).Count());
        }

        /// <summary>
        /// The FindPagesForEntry method should yield no results if the collection doesn't contain entries for the 
        /// requested id.
        /// </summary>
        [TestMethod]
        public void ShouldYieldNoResultsIfCollectionHasTwoPagesAndNoneContainId()
        {
            var col = new DataPageCollection(
                new[]
                    {
                        new DataPageHeader(DataPageType.Items, 0, null, 1, 4, 1, 3, 30),
                        new DataPageHeader(DataPageType.Items, 1, 0, null, 4, 6, 9, 30)
                    });

            Assert.AreEqual(0, col.FindPagesForEntry(4).Count());
            Assert.AreEqual(0, col.FindPagesForEntry(0).Count());
            Assert.AreEqual(0, col.FindPagesForEntry(10).Count());
        }

        /// <summary>
        /// The FindPagesForEntry method should yield no results if the collection doesn't contain entries for the 
        /// requested id.
        /// </summary>
        [TestMethod]
        public void ShouldYieldNoResultsIfCollectionHasMultiplePagesAndNoneContainId()
        {
            var col = new DataPageCollection(
                new[]
                    {
                        new DataPageHeader(DataPageType.Items, 0, null, 1, 4, 1, 3, 30),
                        new DataPageHeader(DataPageType.Items, 1, 0, 2, 4, 6, 9, 30),
                        new DataPageHeader(DataPageType.Items, 2, 1, null, 4, 12, 13, 30)
                    });

            Assert.AreEqual(0, col.FindPagesForEntry(4).Count());
            Assert.AreEqual(0, col.FindPagesForEntry(0).Count());
            Assert.AreEqual(0, col.FindPagesForEntry(10).Count());
            Assert.AreEqual(0, col.FindPagesForEntry(14).Count());
        }

        /// <summary>
        /// The FindPagesForEntry method should yield a page if the collection contains entries for the 
        /// requested id.
        /// </summary>
        [TestMethod]
        public void ShouldYieldOneResultIfCollectionHasOnePageAndItContainsId()
        {
            var headers = new[] { new DataPageHeader(DataPageType.Items, 0, null, null, 4, 1, 3, 30) };
            var col = new DataPageCollection(headers);

            Assert.AreSame(headers[0], col.FindPagesForEntry(1).Single());
            Assert.AreSame(headers[0], col.FindPagesForEntry(2).Single());
            Assert.AreSame(headers[0], col.FindPagesForEntry(3).Single());
        }

        /// <summary>
        /// The FindPagesForEntry method should yield a page if the collection contains only one page that has entries for the 
        /// requested id.
        /// </summary>
        [TestMethod]
        public void ShouldYieldOneResultIfCollectionHasTwoPagesAndContainsIdInSinglePage()
        {
            var headers = new[] 
            { 
                new DataPageHeader(DataPageType.Items, 0, null, 1, 4, 1, 3, 30),
                new DataPageHeader(DataPageType.Items, 1, 0, null, 4, 6, 9, 30)
            };

            var col = new DataPageCollection(headers);

            Assert.AreSame(headers[0], col.FindPagesForEntry(1).Single());
            Assert.AreSame(headers[0], col.FindPagesForEntry(2).Single());
            Assert.AreSame(headers[0], col.FindPagesForEntry(3).Single());

            Assert.AreSame(headers[1], col.FindPagesForEntry(6).Single());
            Assert.AreSame(headers[1], col.FindPagesForEntry(8).Single());
            Assert.AreSame(headers[1], col.FindPagesForEntry(9).Single());
        }

        /// <summary>
        /// The FindPagesForEntry method should yield a page if the collection contains only one page that has entries for the 
        /// requested id.
        /// </summary>
        [TestMethod]
        public void ShouldYieldOneResultIfCollectionHasMultiplePagesAndItContainsIdInSinglePage()
        {
            var headers = new[] 
            { 
                new DataPageHeader(DataPageType.Items, 0, null, 1, 4, 1, 3, 30),
                new DataPageHeader(DataPageType.Items, 1, 0, 2, 4, 6, 9, 30),
                new DataPageHeader(DataPageType.Items, 2, 1, 3, 4, 12, 14, 30),
                new DataPageHeader(DataPageType.Items, 3, 2, null, 4, 19, 21, 30)
            };

            var col = new DataPageCollection(headers);

            Assert.AreSame(headers[0], col.FindPagesForEntry(1).Single());
            Assert.AreSame(headers[0], col.FindPagesForEntry(2).Single());
            Assert.AreSame(headers[0], col.FindPagesForEntry(3).Single());

            Assert.AreSame(headers[1], col.FindPagesForEntry(6).Single());
            Assert.AreSame(headers[1], col.FindPagesForEntry(8).Single());
            Assert.AreSame(headers[1], col.FindPagesForEntry(9).Single());

            Assert.AreSame(headers[2], col.FindPagesForEntry(12).Single());
            Assert.AreSame(headers[2], col.FindPagesForEntry(13).Single());
            Assert.AreSame(headers[2], col.FindPagesForEntry(14).Single());

            Assert.AreSame(headers[3], col.FindPagesForEntry(19).Single());
            Assert.AreSame(headers[3], col.FindPagesForEntry(20).Single());
            Assert.AreSame(headers[3], col.FindPagesForEntry(21).Single());
        }

        /// <summary>
        /// The FindPagesForEntry method should yield a page if the collection contains only one page that has entries for the 
        /// requested id.
        /// </summary>
        [TestMethod]
        public void ShouldYieldMultipleResultIfCollectionHasMultiplePagesAndItContainsIdInMultiplePages()
        {
            var headers = new[] 
            { 
                new DataPageHeader(DataPageType.Items, 0, null, 1, 4, 1, 3, 30),
                new DataPageHeader(DataPageType.Items, 1, 0, 22, 4, 6, 9, 30),
                new DataPageHeader(DataPageType.Items, 22, 1, 3, 4, 9, 9, 30),
                new DataPageHeader(DataPageType.Items, 3, 22, 44, 4, 9, 9, 30),
                new DataPageHeader(DataPageType.Items, 44, 3, 5, 4, 9, 9, 30),
                new DataPageHeader(DataPageType.Items, 5, 44, 6, 4, 9, 21, 30),
                new DataPageHeader(DataPageType.Items, 6, 5, null, 4, 19, 21, 30)
            };

            var col = new DataPageCollection(headers);

            Assert.IsTrue((new[] { 1, 22, 3, 44, 5 }).SequenceEqual(col.FindPagesForEntry(9).Select(h => h.PageNumber)));
        }
    }
}

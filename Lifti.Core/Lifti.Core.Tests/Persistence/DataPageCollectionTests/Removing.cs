// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageCollectionTests
{
    using System;
    using System.Linq;

    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for removing pages from the collection.
    /// </summary>
    [TestClass]
    public class Removing : UnitTestBase
    {
        /// <summary>
        /// An entry should be removable from the end of the collection.
        /// </summary>
        [TestMethod]
        public void ShouldRemoveEndEntry()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();
            var header3 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object, header2.Object, header3.Object });
            collection.Remove(header3.Object);

            Assert.IsTrue((new[] { header1.Object, header2.Object }).SequenceEqual(collection));
        }

        /// <summary>
        /// An entry should be removable from the start of the collection.
        /// </summary>
        [TestMethod]
        public void ShouldRemoveFirstEntry()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();
            var header3 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object, header2.Object, header3.Object });
            collection.Remove(header1.Object);

            Assert.IsTrue((new[] { header2.Object, header3.Object }).SequenceEqual(collection));
        }

        /// <summary>
        /// An entry should be removable from the middle of the collection.
        /// </summary>
        [TestMethod]
        public void ShouldRemoveMiddleEntry()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();
            var header3 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object, header2.Object, header3.Object });
            collection.Remove(header2.Object);

            Assert.IsTrue((new[] { header1.Object, header3.Object }).SequenceEqual(collection));
        }

        /// <summary>
        /// The last entry should be removable from the collection.
        /// </summary>
        [TestMethod]
        public void ShouldRemoveFinalEntry()
        {
            var header1 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object });
            collection.Remove(header1.Object);

            Assert.AreEqual(0, collection.Count);
        }

        /// <summary>
        /// An exception should be thrown if the specified page to remove does not exist in the collection.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfPageToRemoveNotInCollection()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object });
            AssertRaisesException<ArgumentException>(() => collection.Remove(header2.Object), "Page not in list\r\nParameter name: page");
        }
    }
}

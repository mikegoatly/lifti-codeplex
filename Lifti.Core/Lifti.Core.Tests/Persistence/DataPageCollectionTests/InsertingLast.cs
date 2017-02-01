// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageCollectionTests
{
    using System.Linq;

    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for inserting last into a collection.
    /// </summary>
    [TestClass]
    public class InsertingLast : UnitTestBase
    {
        /// <summary>
        /// An entry should be insertable into an empty collection.
        /// </summary>
        [TestMethod]
        public void ShouldInsertIntoEmptyCollection()
        {
            var header1 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection();
            collection.InsertLast(header1.Object);

            Assert.IsTrue((new[] { header1.Object }).SequenceEqual(collection));
        }

        /// <summary>
        /// An entry should be insertable into a populated collection.
        /// </summary>
        [TestMethod]
        public void ShouldInsertIntoPopulatedCollection()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();
            var header3 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object, header2.Object });
            collection.InsertLast(header3.Object);

            Assert.IsTrue((new[] { header1.Object, header2.Object, header3.Object }).SequenceEqual(collection));
        }
    }
}

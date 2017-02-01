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
    /// Tests for inserting items into a <see cref="DataPageCollection"/>.
    /// </summary>
    [TestClass]
    public class Inserting : UnitTestBase
    {
        /// <summary>
        /// An entry should be insertable after the last entry in the collection.
        /// </summary>
        [TestMethod]
        public void ShouldInsertAfterEntryAtEnd()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object });
            collection.Insert(header2.Object, header1.Object);

            Assert.IsTrue((new[] { header1.Object, header2.Object }).SequenceEqual(collection));
        }

        /// <summary>
        /// An entry should be insertable after an entry in the middle of the collection.
        /// </summary>
        [TestMethod]
        public void ShouldInsertAfterEntryInMiddle()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();
            var header3 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object, header2.Object });
            collection.Insert(header3.Object, header1.Object);

            Assert.IsTrue((new[] { header1.Object, header3.Object, header2.Object }).SequenceEqual(collection));
        }

        /// <summary>
        /// An exception should be thrown if the specified page to insert after does not exist in the collection.
        /// </summary>
        [TestMethod]
        public void ShouldThrowExceptionIfPageToInsertAfterNotInCollection()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();
            
            var collection = new DataPageCollection();
            AssertRaisesException<ArgumentException>(() => collection.Insert(header2.Object, header1.Object), "Page not in list\r\nParameter name: afterPage");
        }
    }
}

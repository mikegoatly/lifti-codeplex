// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageCollectionTests
{
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for inserting first into a collection.
    /// </summary>
    [TestFixture]
    public class InsertingFirst
    {
        /// <summary>
        /// An entry should be insertable into an empty collection.
        /// </summary>
        [Test]
        public void ShouldInsertIntoEmptyCollection()
        {
            var header1 = new Mock<IDataPageHeader>();
            
            var collection = new DataPageCollection();
            collection.InsertFirst(header1.Object);

            Assert.IsTrue((new[] { header1.Object }).SequenceEqual(collection));
        }

        /// <summary>
        /// An entry should be insertable into a populated collection.
        /// </summary>
        [Test]
        public void ShouldInsertIntoPopulatedCollection()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();
            var header3 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object, header2.Object });
            collection.InsertFirst(header3.Object);

            Assert.IsTrue((new[] { header3.Object, header1.Object, header2.Object }).SequenceEqual(collection));
        }
    }
}

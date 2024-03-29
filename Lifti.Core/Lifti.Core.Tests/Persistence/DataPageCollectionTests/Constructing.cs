﻿// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataPageCollectionTests
{
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the construction process of the <see cref="DataPageCollection"/> class.
    /// </summary>
    [TestFixture]
    public class Constructing
    {
        /// <summary>
        /// The default constructor should initialize to empty.
        /// </summary>
        [Test]
        public void DefaultConstructorShouldInitializeToEmpty()
        {
            var collection = new DataPageCollection();
            Assert.AreEqual(0, collection.Count);
        }

        /// <summary>
        /// The constructor overload should initialize the collection to the given list.
        /// </summary>
        [Test]
        public void ShouldInitializeToProvidedList()
        {
            var header1 = new Mock<IDataPageHeader>();
            var header2 = new Mock<IDataPageHeader>();

            var collection = new DataPageCollection(new[] { header1.Object, header2.Object });
            Assert.AreEqual(2, collection.Count);
            Assert.IsTrue((new[] { header1.Object, header2.Object }).SequenceEqual(collection));
        }

        /// <summary>
        /// An exception should be thrown if the provided headers are null.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfProvidedHeadersAreNull()
        {
            this.AssertRaisesArgumentNullException(() => new DataPageCollection(null), "headers");
        }
    }
}

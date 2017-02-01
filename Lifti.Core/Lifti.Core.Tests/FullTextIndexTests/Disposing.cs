// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.FullTextIndexTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the disposing of the <see cref="FullTextIndex{TKey}"/>.
    /// </summary>
    [TestClass]
    public class Disposing
    {
        /// <summary>
        /// Tests that disposing an index doesn't raise errors.
        /// </summary>
        [TestMethod]
        public void DisposingAnIndexShouldNotRaiseErrors()
        {
            var index = CreateTestIndex();
            index.Dispose();
        }

        /// <summary>
        /// Tests that disposing an index multiple times doesn't raise errors.
        /// </summary>
        [TestMethod]
        public void DisposingAnIndexMultipleTimesShouldNotRaiseErrors()
        {
            var index = CreateTestIndex();
            index.Dispose();
            index.Dispose();
        }

        /// <summary>
        /// Creates a test index.
        /// </summary>
        /// <returns>The created index.</returns>
        private static FullTextIndex<Customer> CreateTestIndex()
        {
            var index = new FullTextIndex<Customer>();
            var customer = new Customer { Name = "Test", Biography = "Bio text here." };
            index.Index(customer, customer.Biography);
            return index;
        }
    }
}

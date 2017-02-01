// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.FullTextIndexTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the FullTextIndexer class.
    /// </summary>
    [TestClass]
    public abstract class FullTextIndexTestBase : UnitTestBase
    {
        /// <summary>
        /// Creates the full text indexer for the Customer class.
        /// </summary>
        /// <returns>The constructed full text indexer for the customer class.</returns>
        protected static FullTextIndex<Customer> CreateCustomerFullTextIndexer()
        {
            return new FullTextIndex<Customer>();
        }
    }
}

// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.FullTextIndexTests
{
    using NUnit.Framework;

    /// <summary>
    /// Tests for the FullTextIndexer class.
    /// </summary>
    [TestFixture]
    public abstract class FullTextIndexTestBase
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

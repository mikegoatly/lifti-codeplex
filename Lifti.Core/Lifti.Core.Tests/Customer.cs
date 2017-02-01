// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System;

    /// <summary>
    /// A basic customer class to test the indexer with.
    /// </summary>
    [Serializable]
    public class Customer
    {
        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        /// <value>The name of the customer.</value>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the customer's biography.
        /// </summary>
        /// <value>The biography of the customer.</value>
        public string Biography
        {
            get;
            set;
        }
    }
}

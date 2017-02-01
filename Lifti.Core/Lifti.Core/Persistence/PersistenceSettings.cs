// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;

    /// <summary>
    /// The settings provider for the persistence classes within LIFTI.
    /// </summary>
    public class PersistenceSettings : IPersistenceSettings
    {
        /// <summary>
        /// The number of pages to grow when new pages are required.
        /// </summary>
        private int growPageCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceSettings"/> class.
        /// </summary>
        public PersistenceSettings()
        {
            this.GrowPageCount = 10;
        }

        /// <summary>
        /// Gets or sets the number of pages to grow when new pages are required.
        /// </summary>
        /// <value>The number of pages to grow the file by.</value>
        public int GrowPageCount
        {
            get
            {
                return this.growPageCount;
            }

            set
            {
                if (value < 2)
                {
                    throw new ArgumentException("The minimum allowed value for GrowPageCount is 2", nameof(value));
                }

                this.growPageCount = value;
            }
        }
    }
}

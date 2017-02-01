// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;

    /// <summary>
    /// Information about a data page that has been cached in memory.
    /// </summary>
    internal struct CachedPage
    {
        /// <summary>
        /// The date and time the page was last accessed, rounded down to the nearest second.
        /// </summary>
        private DateTime accessDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedPage"/> struct.
        /// </summary>
        /// <param name="page">The cached page.</param>
        public CachedPage(IDataPage page)
            : this()
        {
            this.Page = page;
            this.AccessCount = 1;
            this.LastAccess = DateTime.Now;
        }

        /// <summary>
        /// Gets the cached page.
        /// </summary>
        public IDataPage Page
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the date and time the page was last accessed. When this is set, the time is
        /// rounded down to the nearest second.
        /// </summary>
        /// <value>
        /// The last access date/time.
        /// </value>
        public DateTime LastAccess
        {
            get { return this.accessDate; }
            set { this.accessDate = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second); }
        }

        /// <summary>
        /// Gets or sets the number of times the cached page has been accessed.
        /// </summary>
        /// <value>
        /// The access count.
        /// </value>
        public int AccessCount
        {
            get;
            set;
        }
    }
}

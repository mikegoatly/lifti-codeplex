// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    /// <summary>
    /// The interface implemented by settings providers for the persistence classes.
    /// </summary>
    public interface IPersistenceSettings
    {
        /// <summary>
        /// Gets or sets the number of pages to grow when new pages are required.
        /// </summary>
        /// <value>The number of pages to grow the file by.</value>
        int GrowPageCount
        {
            get;
            set;
        }
    }
}

// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    /// <summary>
    /// The various type of data pages that can be persisted.
    /// </summary>
    public enum DataPageType
    {
        /// <summary>
        /// The page is currently unused.
        /// </summary>
        Unused = 0,

        /// <summary>
        /// The page stores information about items contained in the index.
        /// </summary>
        Items = 1,

        /// <summary>
        /// The page stores information about entries the text index.
        /// </summary>
        IndexNode = 2,

        /// <summary>
        /// The page store information about the index nodes an item is associated to.
        /// </summary>
        ItemNodeIndex = 3
    }

    /// <summary>
    /// The interface implemented by data page headers.
    /// </summary>
    public interface IDataPageHeader
    {
        /// <summary>
        /// Gets or sets the id of the logical page that follows this page.
        /// </summary>
        /// <value>The id of the next page.</value>
        int? NextPage { get; set; }

        /// <summary>
        /// Gets the id of the current page.
        /// </summary>
        /// <value>The id of the current page.</value>
        int PageNumber { get; }

        /// <summary>
        /// Gets or sets the id of the logical page that precedes this page.
        /// </summary>
        /// <value>The id of the previous page.</value>
        int? PreviousPage { get; set; }

        /// <summary>
        /// Gets or sets the entry count for the page.
        /// </summary>
        /// <value>The entry count.</value>
        int EntryCount { get; set; }

        /// <summary>
        /// Gets or sets the type of the data page.
        /// </summary>
        /// <value>The type of the data page.</value>
        DataPageType DataPageType { get; set;  }

        /// <summary>
        /// Gets or sets the internal id of the first entry in the data page.
        /// </summary>
        /// <value>
        /// The internal id of the first entry.
        /// </value>
        int FirstEntry { get; set; }

        /// <summary>
        /// Gets or sets the internal id of the last entry in the data page.
        /// </summary>
        /// <value>
        /// The internal id of the last entry.
        /// </value>
        int LastEntry { get; set; }

        /// <summary>
        /// Gets or sets the current size of the data contained in the page, including the page header.
        /// </summary>
        /// <value>
        /// The current size of the header.
        /// </value>
        short CurrentSize { get; set; }
    }
}
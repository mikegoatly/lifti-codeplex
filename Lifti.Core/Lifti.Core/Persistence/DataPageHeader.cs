// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    /// <summary>
    /// Information about a header associated to a data page.
    /// </summary>
    public class DataPageHeader : IDataPageHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPageHeader"/> class.
        /// </summary>
        /// <param name="dataPageType">The type of the data page.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="entryCount">The number of entries in the page.</param>
        /// <param name="firstId">The internal id of the first entry in the page.</param>
        /// <param name="lastId">The internal id of the last entry in the page.</param>
        /// <param name="currentSize">The current size of the data in the page, including the page header data.</param>
        public DataPageHeader(DataPageType dataPageType, int pageNumber, int? previousPage, int? nextPage, int entryCount, int firstId, int lastId, short currentSize)
        {
            this.DataPageType = dataPageType;
            this.PageNumber = pageNumber;
            this.PreviousPage = previousPage;
            this.NextPage = nextPage;
            this.EntryCount = entryCount;
            this.FirstEntry = firstId;
            this.LastEntry = lastId;
            this.CurrentSize = currentSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPageHeader"/> class.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        internal DataPageHeader(int pageNumber)
        {
            this.PageNumber = pageNumber;
        }

        /// <summary>
        /// Gets or sets the current size of the data contained in the page, including the page header.
        /// </summary>
        /// <value>
        /// The current size of the header.
        /// </value>
        public short CurrentSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id of the logical page that follows this page.
        /// </summary>
        /// <value>The id of the next page.</value>
        public int? NextPage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the id of the current page.
        /// </summary>
        /// <value>The id of the current page.</value>
        public int PageNumber
        {
            get; }

        /// <summary>
        /// Gets or sets the entry count for the page.
        /// </summary>
        /// <value>The entry count.</value>
        public int EntryCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id of the logical page that precedes this page.
        /// </summary>
        /// <value>The id of the previous page.</value>
        public int? PreviousPage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the data page.
        /// </summary>
        /// <value>The type of the data page.</value>
        public DataPageType DataPageType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the internal id of the first entry in the data page.
        /// </summary>
        /// <value>
        /// The internal id of the first entry.
        /// </value>
        public int FirstEntry
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the internal id of the last entry in the data page.
        /// </summary>
        /// <value>
        /// The internal id of the last entry.
        /// </value>
        public int LastEntry
        {
            get;
            set;
        }
    }
}

// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System.IO;
    using System.Linq;

    using Lifti.Persistence;

    /// <summary>
    /// The base class for page setups - also used to reference an empty page.
    /// </summary>
    public class PageSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageSetup"/> class.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        public PageSetup(int pageNumber, int? previousPage, int? nextPage)
        {
            this.PageNumber = pageNumber;
            this.PreviousPage = previousPage;
            this.NextPage = nextPage;
        }

        /// <summary>
        /// Gets the page number.
        /// </summary>
        /// <value>The page number.</value>
        public int PageNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the previous page.
        /// </summary>
        /// <value>The previous page.</value>
        protected int? PreviousPage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the next page.
        /// </summary>
        /// <value>The next page.</value>
        protected int? NextPage
        {
            get;
            private set;
        }

        /// <summary>
        /// Writes this instance to the given memory stream.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to write to.</param>
        public void WriteTo(MemoryStream stream)
        {
            var writer = new BinaryWriter(stream);
            this.WriteTo(writer);
        }

        /// <summary>
        /// Writes this instance to the given writer.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        protected virtual void WriteTo(BinaryWriter writer)
        {
            writer.Write(Data.Start.Then((byte)DataPageType.Unused).Then(-1, -1).Then((short)0).Then((short)Data.PageHeaderSize).ToArray());
        }
    }
}
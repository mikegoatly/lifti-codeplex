// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Information about a node index page, setup as part of a test.
    /// </summary>
    public class IndexNodePageSetup : PageSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexNodePageSetup"/> class.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="referencedItems">The referenced items in the page.</param>
        public IndexNodePageSetup(int pageNumber, int? previousPage, int? nextPage, RefSetupBase referencedItems)
            : base(pageNumber, previousPage, nextPage)
        {
            this.ReferencedItems = referencedItems;
        }

        /// <summary>
        /// Gets the referenced items in the page.
        /// </summary>
        /// <value>The referenced items.</value>
        public RefSetupBase ReferencedItems { get; }

        /// <summary>
        /// Writes this instance to the given writer.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        protected override void WriteTo(BinaryWriter writer)
        {
            var hasItems = this.ReferencedItems != null && this.ReferencedItems.Count > 0;
            writer.Write(
                Data.IndexNodePage(
                    this.PreviousPage, 
                    this.NextPage, 
                    (short)(hasItems ? this.ReferencedItems.Count : 0), 
                    hasItems ? this.ReferencedItems.References.First().Id : 0, 
                    hasItems ? this.ReferencedItems.References.Last().Id : 0,
                    (short)(Data.PageHeaderSize + (hasItems ? this.ReferencedItems.References.Sum(i => i.Size) : 0))).ToArray());

            if (hasItems)
            {
                foreach (var item in this.ReferencedItems.References)
                {
                    item.WriteTo(writer);
                }
            }
        }
    }
}
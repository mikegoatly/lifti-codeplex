// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Information about an item index page, setup as part of a test.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class ItemIndexPageSetup<TItem> : PageSetup
    {
        /// <summary>
        /// The items contained within the page.
        /// </summary>
        private readonly IndexedItemSetup<TItem> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemIndexPageSetup&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="items">The items contained in the page.</param>
        public ItemIndexPageSetup(int pageNumber, int? previousPage, int? nextPage, IndexedItemSetup<TItem> items)
            : base(pageNumber, previousPage, nextPage)
        {
            this.items = items;
        }

        /// <summary>
        /// Writes this instance to the given writer.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        protected override void WriteTo(BinaryWriter writer)
        {
            var hasItems = this.items != null && this.items.Count > 0;
            writer.Write(
                Data.ItemPage(
                    this.PreviousPage, 
                    this.NextPage, 
                    (short)(hasItems ? this.items.Count : 0), 
                    hasItems ? this.items.Items.First().Id : 0, 
                    hasItems ? this.items.Items.Last().Id : 0,
                    (short)(Data.PageHeaderSize + (hasItems ? this.items.Items.Sum(i => i.Size) : 0))).ToArray());

            if (hasItems)
            {
                foreach (var item in this.items.Items)
                {
                    item.WriteTo(writer);
                }
            }
        }
    }
}
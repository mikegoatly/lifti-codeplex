// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Collections.Generic;

    /// <summary>
    /// A data page that contains information about the items referenced in the index.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class ItemIndexDataPage<TItem> : DataPage<ItemEntry<TItem>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemIndexDataPage{TItem}"/> class.
        /// </summary>
        /// <param name="header">The page header.</param>
        /// <param name="entries">The entries contained in the page.</param>
        public ItemIndexDataPage(IDataPageHeader header, IEnumerable<ItemEntry<TItem>> entries)
            : base(header, entries)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemIndexDataPage{TItem}"/> class.
        /// </summary>
        /// <param name="header">The page header.</param>
        internal ItemIndexDataPage(IDataPageHeader header)
            : base(header)
        {
        }
    }
}

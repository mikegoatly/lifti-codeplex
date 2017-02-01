// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Collections.Generic;

    /// <summary>
    /// A data page that contains information about the relationships between an indexed item and the
    /// index nodes it is indexed against.
    /// </summary>
    public class ItemNodeIndexDataPage : DataPage<ItemNodeIndexEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNodeIndexDataPage"/> class.
        /// </summary>
        /// <param name="header">The page header.</param>
        /// <param name="entries">The entries contained in the page.</param>
        public ItemNodeIndexDataPage(IDataPageHeader header, IEnumerable<ItemNodeIndexEntry> entries)
            : base(header, entries)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNodeIndexDataPage"/> class.
        /// </summary>
        /// <param name="header">The page header.</param>
        internal ItemNodeIndexDataPage(IDataPageHeader header)
            : base(header)
        {
        }
    }
}

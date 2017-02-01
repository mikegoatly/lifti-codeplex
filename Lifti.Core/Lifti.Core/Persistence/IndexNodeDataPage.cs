// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Collections.Generic;

    /// <summary>
    /// A data page that contains information about nodes in the full text index.
    /// </summary>
    public class IndexNodeDataPage : DataPage<IndexNodeEntryBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexNodeDataPage"/> class.
        /// </summary>
        /// <param name="header">The page header.</param>
        /// <param name="entries">The entries in the page.</param>
        public IndexNodeDataPage(IDataPageHeader header, IEnumerable<IndexNodeEntryBase> entries)
            : base(header, entries)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexNodeDataPage"/> class.
        /// </summary>
        /// <param name="header">The page header.</param>
        internal IndexNodeDataPage(IDataPageHeader header)
            : base(header)
        {
        }
    }
}

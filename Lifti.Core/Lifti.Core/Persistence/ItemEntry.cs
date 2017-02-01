// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Diagnostics;

    /// <summary>
    /// A data entry that relates an item that is indexed within the full text index to its logical id within the persisted file store.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    [DebuggerDisplay("{Id}: {Item}")]
    public class ItemEntry<TItem> : DataPageEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemEntry&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="id">The internal id of the entry.</param>
        /// <param name="item">The item stored in the index.</param>
        /// <param name="size">The size of the item in bytes.</param>
        public ItemEntry(int id, TItem item, short size)
            : base(id, size)
        {
            this.Item = item;
        }

        /// <summary>
        /// Gets the indexed item.
        /// </summary>
        /// <value>The indexed item.</value>
        public TItem Item
        {
            get; }
    }
}

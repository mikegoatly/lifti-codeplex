// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Extensibility
{
    /// <summary>
    /// Event arguments that contain a reference to an item and the node it was indexed against in the full text index.
    /// </summary>
    /// <typeparam name="TItem">The type of item/key stored in the full text index.</typeparam>
    public class ItemNodeEventArgs<TItem> : IndexNodeEventArgs<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNodeEventArgs&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="node">The associated node.</param>
        /// <param name="item">The indexed item.</param>
        public ItemNodeEventArgs(IndexNode<TItem> node, TItem item)
            : base(node)
        {
            this.Item = item;
        }

        /// <summary>
        /// Gets or sets the item responsible for the event being raised.
        /// </summary>
        /// <value>
        /// The associated item.
        /// </value>
        public TItem Item
        {
            get;
            set;
        }
    }
}
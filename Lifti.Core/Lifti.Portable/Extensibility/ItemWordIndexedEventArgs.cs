// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Extensibility
{
    /// <summary>
    /// Event arguments that contain a reference to an <see cref="Lifti.ItemWordMatch&lt;TKey&gt;"/> and the node it was indexed against in the full text index.
    /// </summary>
    /// <typeparam name="TItem">The type of item/key stored in the full text index.</typeparam>
    public class ItemWordIndexedEventArgs<TItem> : IndexNodeEventArgs<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemWordIndexedEventArgs&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="node">The associated node.</param>
        /// <param name="itemWordMatch">The indexed item word match.</param>
        public ItemWordIndexedEventArgs(IndexNode<TItem> node, ItemWordMatch<TItem> itemWordMatch)
            : base(node)
        {
            this.ItemWordMatch = itemWordMatch;
        }

        /// <summary>
        /// Gets or sets the item word match responsible for the event being raised.
        /// </summary>
        /// <value>
        /// The associated item word match.
        /// </value>
        public ItemWordMatch<TItem> ItemWordMatch
        {
            get;
            set;
        }
    }
}
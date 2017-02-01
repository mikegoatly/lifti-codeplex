// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Extensibility
{
    #region Using statements

    using System;

    #endregion

    /// <summary>
    /// Event arguments that contain a reference to an item stored in the index.
    /// </summary>
    /// <typeparam name="TItem">The type of item/key stored in the full text index.</typeparam>
    public class ItemEventArgs<TItem> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemEventArgs&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="item">The associated item.</param>
        public ItemEventArgs(TItem item)
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
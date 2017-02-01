// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Extensibility
{
    using System;

    /// <summary>
    /// Event arguments that contain a reference to a node in the full text index.
    /// </summary>
    /// <typeparam name="TItem">The type of item/key stored in the full text index.</typeparam>
    public class IndexNodeEventArgs<TItem> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexNodeEventArgs&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="node">The associated node.</param>
        public IndexNodeEventArgs(IndexNode<TItem> node)
        {
            this.Node = node;
        }

        /// <summary>
        /// Gets or sets the index node responsible for the event being raised.
        /// </summary>
        /// <value>
        /// The associated node.
        /// </value>
        public IndexNode<TItem> Node
        {
            get;
            set;
        }
    }
}
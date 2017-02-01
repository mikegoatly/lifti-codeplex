// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System;

    /// <summary>
    /// The base LIFTI exception class.
    /// </summary>

    public class LiftiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiftiException"/> class.
        /// </summary>
        public LiftiException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiftiException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public LiftiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiftiException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public LiftiException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

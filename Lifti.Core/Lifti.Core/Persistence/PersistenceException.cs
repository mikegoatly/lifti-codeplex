// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence  
{
    #region Using statements

    using System;
    using System.Globalization;

    #endregion

    /// <summary>
    /// The <see cref="PersistenceException"/> may be thrown by various parts of the persistence code.
    /// </summary>
    public class PersistenceException : LiftiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceException"/> class.
        /// </summary>
        public PersistenceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public PersistenceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="args">The message arguments.</param>
        public PersistenceException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public PersistenceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

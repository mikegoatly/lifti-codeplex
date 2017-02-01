// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The <see cref="QueryParserException"/> may be thrown by various parts of the query parsing code.
    /// </summary>

    public class QueryParserException : LiftiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParserException"/> class.
        /// </summary>
        public QueryParserException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParserException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public QueryParserException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParserException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="args">The message arguments.</param>
        public QueryParserException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParserException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public QueryParserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

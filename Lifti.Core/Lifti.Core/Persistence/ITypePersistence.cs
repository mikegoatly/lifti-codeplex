// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.IO;

    /// <summary>
    /// The interface implemented by classes capable of persisting and retrieving data for a particular type 
    /// to/from <see cref="BinaryWriter"/>s and <see cref="BinaryReader"/>s.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public interface ITypePersistence<TItem>
    {
        /// <summary>
        /// Gets the size reader that will be used for the type of TItem.
        /// </summary>
        /// <value>The size reader.</value>
        Func<TItem, short> SizeReader { get; }

        /// <summary>
        /// Gets a value indicating whether the type TItem has a dynamic size, as in the case of <see cref="String"/>.
        /// </summary>
        /// <value><c>true</c> if the type TItem has a dynamic size; otherwise, <c>false</c>.</value>
        bool TypeHasDynamicSize { get; }

        /// <summary>
        /// Gets a delegate capable of writing a TItem instance to a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <value>The action capable of writing a TItem to a <see cref="BinaryWriter"/> instance.</value>
        Action<BinaryWriter, TItem> DataWriter { get; }

        /// <summary>
        /// Gets a delegate capable of reading a TItem instance from a <see cref="BinaryReader"/>.
        /// </summary>
        /// <value>The function capable of reading a TItem from a <see cref="BinaryReader"/> instance.</value>
        Func<BinaryReader, TItem> DataReader { get; }
    }
}
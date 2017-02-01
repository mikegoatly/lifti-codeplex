// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.IO;

    /// <summary>
    /// The base class for type persistence classes.
    /// </summary>
    /// <typeparam name="TItem">The type of the item being persisted.</typeparam>
    public class TypePersistenceBase<TItem> : ITypePersistence<TItem>
    {
        /// <summary>
        /// Gets the size reader that will be used for the type of TItem.
        /// </summary>
        /// <value>The size reader.</value>
        public Func<TItem, short> SizeReader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a value indicating whether the type TItem has a dynamic size, as in the case of <see cref="String"/>.
        /// </summary>
        /// <value><c>true</c> if the type TItem has a dynamic size; otherwise, <c>false</c>.</value>
        public bool TypeHasDynamicSize
        {
            get; }

        /// <summary>
        /// Gets a delegate capable of writing a TItem instance to a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <value>The action capable of writing a TItem to a <see cref="BinaryWriter"/> instance.</value>
        public Action<BinaryWriter, TItem> DataWriter
        {
            get; }

        /// <summary>
        /// Gets a delegate capable of reading a TItem instance from a <see cref="BinaryReader"/>.
        /// </summary>
        /// <value>The function capable of reading a TItem from a <see cref="BinaryReader"/> instance.</value>
        public Func<BinaryReader, TItem> DataReader
        {
            get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypePersistenceBase{TItem}"/> class.
        /// </summary>
        /// <param name="dataWriter">A delegate capable of writing an instance to a <see cref="BinaryWriter"/>.</param>
        /// <param name="dataReader">A delegate capable of reading an instance from a <see cref="BinaryReader"/>.</param>
        internal TypePersistenceBase(Action<BinaryWriter, TItem> dataWriter, Func<BinaryReader, TItem> dataReader)
        {
            this.DataWriter = dataWriter;
            this.DataReader = dataReader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypePersistenceBase{TItem}"/> class.
        /// </summary>
        /// <param name="typeHasDynamicSize">Whether or not the size of the persisted type varies from instance to instance.</param>
        /// <param name="sizeReader">A delegate capable of reading the size of a particular instance of the type.</param>
        /// <param name="dataWriter">A delegate capable of writing an instance to a <see cref="BinaryWriter"/>.</param>
        /// <param name="dataReader">A delegate capable of reading an instance from a <see cref="BinaryReader"/>.</param>
        protected TypePersistenceBase(bool typeHasDynamicSize, Func<TItem, short> sizeReader, Action<BinaryWriter, TItem> dataWriter, Func<BinaryReader, TItem> dataReader)
            : this(dataWriter, dataReader)
        {
            this.TypeHasDynamicSize = typeHasDynamicSize;
            this.SizeReader = sizeReader;
        }
    }
}
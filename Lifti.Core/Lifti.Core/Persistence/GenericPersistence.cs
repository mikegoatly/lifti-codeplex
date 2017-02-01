// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    #region Using statements

    using System.IO;
    using System.Reflection;

    #endregion

    /// <summary>
    /// A generic persistence class for primitive types.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class GenericPersistence<TItem> : TypePersistenceBase<TItem>
    {
        /// <summary>
        /// The size of the type when written using a binary writer.
        /// </summary>
        private readonly short typeSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPersistence{TItem}"/> class.
        /// </summary>
        public GenericPersistence()
            : base(SerializationHelper.DetermineWriterMethod<TItem>(), SerializationHelper.DetermineReaderMethod<TItem>())
        {
            if (this.DataReader == null || this.DataWriter == null || !typeof(TItem).GetTypeInfo().IsPrimitive)
            {
                throw new PersistenceException("Unable to automatically serialize type " + typeof(TItem).Name);
            }

            // Measure the size of the type
            using (var writer = new BinaryWriter(new MemoryStream(16)))
            {
                this.DataWriter(writer, default(TItem));
                this.typeSize = (short)writer.BaseStream.Length;
                this.SizeReader = i => this.typeSize;
            }
        }
    }
}

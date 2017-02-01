// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

#if NETFX_CORE
    using System.Reflection;
#endif

    /// <summary>
    /// Helper methods for the serialization process.
    /// </summary>
    internal static class SerializationHelper
    {
        /// <summary>
        /// Determines the Write method on the <see cref="BinaryWriter"/> class that handles the type of T and creates
        /// a compiled action that writes the key value to a binary writer.
        /// </summary>
        /// <typeparam name="T">The type of the item that will be written to the <see cref="BinaryWriter"/>.</typeparam>
        /// <returns>
        /// The compiled action, or null if the writer does not support writing the type
        /// directly.
        /// </returns>
        internal static Action<BinaryWriter, T> DetermineWriterMethod<T>()
        {
            var writerType = typeof(BinaryWriter);
            var method = writerType.GetTypeInfo()
                .GetDeclaredMethods("Write")
                .FirstOrDefault(m => m.GetParameters().Count() == 1 && m.GetParameters()[0].ParameterType ==  typeof(T));

            if (method != null)
            {
                var writerParameter = Expression.Parameter(writerType, "writer");
                var keyParameter = Expression.Parameter(typeof(T), "key");
                var call = Expression.Call(writerParameter, method, keyParameter);
                return Expression.Lambda<Action<BinaryWriter, T>>(call, writerParameter, keyParameter).Compile();
            }

            return null;
        }

        /// <summary>
        /// Determines the Read method on the <see cref="BinaryReader"/> class that handles the type of T and creates
        /// a compiled action that writes the key value to a binary writer.
        /// </summary>
        /// <typeparam name="T">The type of the item that will be read from the <see cref="BinaryWriter"/>.</typeparam>
        /// <returns>
        /// The compiled action, or null if the reader does not support reading the type
        /// directly.
        /// </returns>
        internal static Func<BinaryReader, T> DetermineReaderMethod<T>()
        {
            var readerType = typeof(BinaryReader);
            var method = readerType.GetTypeInfo().GetDeclaredMethods("Read" + typeof(T).Name).FirstOrDefault();

            if (method != null)
            {
                var readerParameter = Expression.Parameter(readerType, "reader");
                var call = Expression.Call(readerParameter, method);
                return Expression.Lambda<Func<BinaryReader, T>>(call, readerParameter).Compile();
            }

            return null;
        }
    }
}

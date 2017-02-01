// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// The type persistence implementation for strings.
    /// </summary>
    public class StringPersistence : TypePersistenceBase<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringPersistence"/> class.
        /// </summary>
        public StringPersistence()
            : base(WriteString, ReadString)
        {
            this.SizeReader = s => (short)(Encoding.UTF8.GetByteCount(s) + 2);
        }

        /// <summary>
        /// Reads a string.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The read string.</returns>
        private static string ReadString(BinaryReader reader)
        {
            var length = reader.ReadInt16();
            return Encoding.UTF8.GetString(reader.ReadBytes(length), 0, length);
        }

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to write.</param>
        private static void WriteString(BinaryWriter writer, string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            writer.Write((short)data.Length);
            writer.Write(data);
        }
    }
}
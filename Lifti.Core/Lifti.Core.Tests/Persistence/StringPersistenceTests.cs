﻿// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence
{
    using System.IO;
    using System.Linq;
    using System.Text;

    using Lifti.Persistence;

    using NUnit.Framework;

    /// <summary>
    /// Tests for using the string persistence class.
    /// </summary>
    [TestFixture]
    public class StringPersistenceTests
    {
        /// <summary>
        /// The class should successfully return a string from the stream.
        /// </summary>
        [Test]
        public void ShouldReturnString()
        {
            using (var stream = new MemoryStream(Data.Start.Then((short)4).Then(Encoding.UTF8.GetBytes("Test")).ToArray()))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var persistence = new StringPersistence();
                    Assert.AreEqual("Test", persistence.DataReader(reader));
                }
            }
        }

        /// <summary>
        /// The class should write a string to the stream.
        /// </summary>
        [Test]
        public void ShouldWriteString()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    var persistence = new StringPersistence();
                    persistence.DataWriter(writer, "Test");

                    Assert.IsTrue(stream.ToArray().SequenceEqual(Data.Start.Then((short)4).Then(Encoding.UTF8.GetBytes("Test"))));
                }
            }
        }

        /// <summary>
        /// The class should measure a string correctly.
        /// </summary>
        [Test]
        public void ShouldMeasureString()
        {
            var persistence = new StringPersistence();
            Assert.AreEqual(6, persistence.SizeReader("Test"));
        }
    }
}

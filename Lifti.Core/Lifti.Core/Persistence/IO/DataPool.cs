// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// A static pool of memory blocks.
    /// </summary>
    internal static class DataPool
    {
        /// <summary>
        /// The lookup of byte arrays against their 
        /// </summary>
        private static readonly Dictionary<int, Queue<byte[]>> dataLookup = new Dictionary<int, Queue<byte[]>>();

        /// <summary>
        /// The thread synchronization object.
        /// </summary>
        private static readonly object syncObj = new object();

        /// <summary>
        /// A semaphore to prevent too many blocks of data being open at once. This is also a safety check
        /// to prevent a bug in code allocating, but never de-allocating data, as this barrier will eventually
        /// cause an exception to be thrown.
        /// </summary>
        private static readonly Semaphore maxAllocationBarrier = new Semaphore(20, 20);

        /// <summary>
        /// Allocates data of the requested size from the pool or creates it if required.
        /// </summary>
        /// <param name="size">The size of the data to request.</param>
        /// <returns>The allocated data.</returns>
        internal static byte[] AllocateData(int size)
        {
            if (!maxAllocationBarrier.WaitOne(1000))
            {
                throw new PersistenceException("Unable to allocate memory - its possible that it is not being properly de-allocated.");
            }

            byte[] data = null;
            lock (syncObj)
            {
                var dataList = GetOrCreateDataQueue(size);
                if (dataList.Count > 0)
                {
                    data = dataList.Dequeue();
                }
            }

            return data ?? new byte[size];
        }

        /// <summary>
        /// Pools the data, caching it for later use.
        /// </summary>
        /// <param name="data">The data to pool.</param>
        internal static void PoolData(byte[] data)
        {
            maxAllocationBarrier.Release();

            lock (syncObj)
            {
                GetOrCreateDataQueue(data.Length).Enqueue(data);
            }
        }

        /// <summary>
        /// Gets a queue of data from the dictionary, or creates one if needed.
        /// </summary>
        /// <param name="size">The size of the data to get the queue for.</param>
        /// <returns>The data queue.</returns>
        private static Queue<byte[]> GetOrCreateDataQueue(int size)
        {
            Queue<byte[]> data;
            if (!dataLookup.TryGetValue(size, out data))
            {
                data = new Queue<byte[]>();
                dataLookup.Add(size, data);
            }

            return data;
        }
    }
}

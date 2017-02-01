// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    using System;
    using System.IO;
    using System.Linq;

    // TODO Is this needed without transaction support?
    /// <summary>
    /// The file manager for the underlying log file.
    /// </summary>
    internal class LogFileManager : FileManagerBase, ILogFileManager
    {
        /// <summary>
        /// The log file version.
        /// </summary>
        private const byte LogVersion = 1;

        /// <summary>
        /// The size of the log file header.
        /// </summary>
        private const int LogHeaderSize = 12;

        /// <summary>
        /// The common marker bytes that are stored in the header of the file.
        /// </summary>
        private static readonly byte[] headerBytes = { 0x4C, 0x49, 0x4C, 0x47, 0x4D, 0x47 };

        /// <summary>
        /// The state of the current transaction log.
        /// </summary>
        private TransactionLogState logState;

        /// <summary>
        /// The binary reader for this instance;
        /// </summary>
        private BinaryReader reader;

        /// <summary>
        /// The binary writer for this instance.
        /// </summary>
        private BinaryWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFileManager"/> class.
        /// </summary>
        /// <param name="logFileName">The log file path.</param>
        public LogFileManager(string logFileName)
            : base(logFileName)
        {
            this.reader = new BinaryReader(this.DataStream);
            this.writer = new BinaryWriter(this.DataStream);

            if (this.CurrentLength >= LogHeaderSize)
            {
                // Verify the header and read the current log state.
                if (!headerBytes.SequenceEqual(this.reader.ReadBytes(headerBytes.Length)))
                {
                    // We could just overwrite the file, but we could be overwriting something that doesn't belong to us...
                    throw new PersistenceException("Existing log file does not contain expected header bytes.");
                }

                var version = this.reader.ReadByte();
                this.logState = (TransactionLogState)this.reader.ReadByte();

                // If there is an outstanding transaction, make sure the version of the log file is compatible
                if (this.LogState != TransactionLogState.None &&
                    this.LogState != TransactionLogState.TransactionCommitted && version != LogVersion)
                {
                    throw new PersistenceException(
                        "An incompatible log file exists for a transaction that needs to be rolled back.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the current state of the log file.
        /// </summary>
        /// <value>The current state of the log file.</value>
        public TransactionLogState LogState
        {
            get
            {
                return this.logState;
            }

            set
            {
                if (value != this.logState)
                {
                    lock (this.SyncObj)
                    {
                        // Keep track of where the data stream was
                        var position = this.DataStream.Position;

                        this.DataStream.Position = 7;
                        this.writer.Write((byte)value);

                        // Reset the position of the data stream
                        this.DataStream.Position = position;
                    }

                    this.logState = value;
                }
            }
        }

        /// <summary>
        /// Gets the extent of the original data file before the logged transaction started.
        /// </summary>
        /// <value>
        /// The extent of the original data file before the logged transaction started.
        /// </value>
        public int OriginalDataFileExtent
        {
            get
            {
                if (this.CurrentLength < LogHeaderSize)
                {
                    throw new InvalidOperationException("Unable to read original data file extent from log - no extent data logged.");
                }

                int originalExtent;
                lock (this.SyncObj)
                {
                    // Keep track of where the data stream was
                    var position = this.DataStream.Position;

                    // The original extent is the last 4 bytes in the log header
                    this.DataStream.Position = LogHeaderSize - 4;
                    originalExtent = this.reader.ReadInt32();

                    // Reset the position of the data stream
                    this.DataStream.Position = position;
                }

                return originalExtent;
            }
        }

        /// <summary>
        /// Initializes the log file in preparation for logging data from the given data file manager.
        /// </summary>
        /// <param name="dataFileManager">The data file manager.</param>
        public void InitializeNewLog(IDataFileManager dataFileManager)
        {
            if (dataFileManager == null)
            {
                throw new ArgumentNullException(nameof(dataFileManager));
            }

            // Store directly in the variable bypassing the write logic in the setter
            this.logState = TransactionLogState.Incomplete;

            this.DataStream.Position = 0;
            this.writer.Write(headerBytes);
            this.writer.Write(LogVersion);
            this.writer.Write((byte)TransactionLogState.Incomplete);
            this.writer.Write(dataFileManager.CurrentLength);
        }

        /// <summary>
        /// Writes the end marker to the log.
        /// </summary>
        public void EndLog()
        {
            if (this.LogState != TransactionLogState.Incomplete)
            {
                throw new PersistenceException("Transaction log in an invalid state to end logging - current state: " + this.LogState);
            }

            // Write the end of log entry
            this.writer.Write((byte)LogEntryDataType.EndOfLog);

            // Write that the log has been written, but not committed yet.
            this.LogState = TransactionLogState.TransactionLogged;
        }

        /// <summary>
        /// Logs the data from the given data file manager to the log file.
        /// </summary>
        /// <param name="entryType">The type of the entry being logged.</param>
        /// <param name="dataFileManager">The data file manager to log the data from.</param>
        /// <param name="offset">The offset of the data to read.</param>
        /// <param name="length">The length of the data to log.</param>
        public void LogDataFrom(LogEntryDataType entryType, IDataFileManager dataFileManager, int offset, int length)
        {
            if (dataFileManager == null)
            {
                throw new ArgumentNullException(nameof(dataFileManager));
            }

            if (this.LogState != TransactionLogState.Incomplete)
            {
                throw new PersistenceException("Transaction log in an invalid state to log data - current state: " + this.LogState);
            }

            this.writer.Write((byte)entryType);
            this.writer.Write(offset);
            this.writer.Write(length);
            var data = new byte[length];
            this.writer.Write(dataFileManager.ReadRaw(offset, data));
        }

        /// <summary>
        /// Rollbacks the logged data to the given data file manager.
        /// </summary>
        /// <param name="dataFileManager">The data file manager to rollback the logged data to.</param>
        public void RollbackDataTo(IDataFileManager dataFileManager)
        {
            if (dataFileManager == null)
            {
                throw new ArgumentNullException(nameof(dataFileManager));
            }

            if (this.LogState == TransactionLogState.None || this.LogState == TransactionLogState.TransactionCommitted)
            {
                throw new PersistenceException("Transaction log in an invalid state to rollback data - current state: " + this.LogState);
            }

            this.DataStream.Position = LogHeaderSize;

            LogEntryDataType entryType;
            do
            {
                entryType = (LogEntryDataType)this.reader.ReadByte();
                switch (entryType)
                {
                    case LogEntryDataType.FullPage:
                    case LogEntryDataType.PageHeader:
                    case LogEntryDataType.PageManagerHeader:
                        var offset = this.reader.ReadInt32();
                        var length = this.reader.ReadInt32();
                        dataFileManager.WriteRaw(offset, this.reader.ReadBytes(length), length);
                        break;

                    case LogEntryDataType.EndOfLog:
                        break;

                    default:
                        throw new PersistenceException("Unexpected log entry in log file - the log file is possibly corrupted");
                }
            }
            while (entryType != LogEntryDataType.EndOfLog);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.reader != null)
                {
                    this.reader.Dispose();
                    this.reader = null;
                }

                if (this.writer != null)
                {
                    this.writer.Dispose();
                    this.writer = null;
                }
            }
        }
    }
}
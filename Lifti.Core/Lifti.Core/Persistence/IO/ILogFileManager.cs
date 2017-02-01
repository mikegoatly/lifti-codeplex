// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    using System;

    /// <summary>
    /// The interface implemented by classes capable of managing the underlying data stream for
    /// a log file.
    /// </summary>
    public interface ILogFileManager : IFileManager, IDisposable
    {
        /// <summary>
        /// Gets or sets the current state of the log file.
        /// </summary>
        /// <value>The current state of the log file.</value>
        TransactionLogState LogState { get; set; }

        /// <summary>
        /// Gets the extent of the original data file before the logged transaction started.
        /// </summary>
        /// <value>The extent of the original data file before the logged transaction started.</value>
        int OriginalDataFileExtent { get; }

        /// <summary>
        /// Writes the end marker to the log.
        /// </summary>
        void EndLog();

        /// <summary>
        /// Logs the data from the given data file manager to the log file.
        /// </summary>
        /// <param name="entryType">The type of the entry being logged.</param>
        /// <param name="dataFileManager">The data file manager to log the data from.</param>
        /// <param name="offset">The offset of the data to read.</param>
        /// <param name="length">The length of the data to log.</param>
        void LogDataFrom(LogEntryDataType entryType, IDataFileManager dataFileManager, int offset, int length);

        /// <summary>
        /// Initializes the log file in preparation for logging data from the given data file manager.
        /// </summary>
        /// <param name="dataFileManager">The data file manager.</param>
        void InitializeNewLog(IDataFileManager dataFileManager);

        /// <summary>
        /// Rollbacks the logged data to the given data file manager.
        /// </summary>
        /// <param name="dataFileManager">The data file manager to rollback the logged data to.</param>
        void RollbackDataTo(IDataFileManager dataFileManager);
    }
}

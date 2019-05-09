using System.IO;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Implementation interface for a download task.
    /// </summary>
    public interface IDownloadTaskImpl
    {
        /// <summary>
        /// Whether the download task is done.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// The size really downloaded by the current task.
        /// </summary>
        long RealDownloadedSize { get; }

        /// <summary>
        /// The chunk size in bytes to save to the disk. A value that is less than or equal to 0 means the download won't be chunk based.
        /// </summary>
        int ChunkSizeToSave { get; set; }

        /// <summary>
        /// Error code.
        /// </summary>
        DownloadErrorCode? ErrorCode { get; }

        /// <summary>
        /// Error message.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Callback when a download task is reset.
        /// </summary>
        void OnReset();

        /// <summary>
        /// Callback when a download task is started.
        /// </summary>
        /// <param name="urlStr">Url string from which to download.</param>
        /// <param name="startByteIndex">The starting byte index to download from.</param>
        void OnStart(string urlStr, long startByteIndex);

        /// <summary>
        /// Callback when a download task is stopped.
        /// </summary>
        void OnStop();

        /// <summary>
        /// Callback when a download task runs out of time.
        /// </summary>
        void OnTimeOut();

        /// <summary>
        /// Callback when a download error occurs (regarding the network, web or so).
        /// </summary>;
        void OnDownloadError();

        /// <summary>
        /// Write downloaded content to given writer.
        /// </summary>
        /// <param name="bw">Binary writer to write the content.</param>
        /// <param name="offset">Where to start.</param>
        /// <param name="size">How many bytes to write.</param>
        void WriteDownloadedContent(BinaryWriter bw, long offset, long size);

        /// <summary>
        /// Generic tick method.
        /// </summary>
        /// <param name="timeStruct">Time struct.</param>
        void Update(TimeStruct timeStruct);
    }
}
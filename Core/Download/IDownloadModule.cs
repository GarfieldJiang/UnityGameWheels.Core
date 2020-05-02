namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface of a download module.
    /// </summary>
    public interface IDownloadModule : IModule
    {
        /// <summary>
        /// Factory for a <see cref="IDownloadTaskImpl"/> instance.
        /// </summary>
        ISimpleFactory<IDownloadTaskImpl> DownloadTaskImplFactory { get; set; }

        /// <summary>
        /// Get or set the object pool module.
        /// </summary>
        IRefPoolService RefPoolService { get; set; }

        /// <summary>
        /// Download task pool.
        /// </summary>
        IDownloadTaskPool DownloadTaskPool { get; set; }

        /// <summary>
        /// Temporary file extension, starting with a fullstop.
        /// </summary>
        string TempFileExtension { get; set; }

        /// <summary>
        /// The upper limit of the number of concurrent downloading tasks.
        /// </summary>
        int ConcurrentDownloadCountLimit { get; set; }

        /// <summary>
        /// The chunk size in bytes to save to the disk. A value that is less than or equal to 0 means the download won't be chunk based.
        /// </summary>
        int ChunkSizeToSave { get; set; }

        /// <summary>
        /// Default time limit of any task.
        /// </summary>
        float Timeout { get; set; }

        /// <summary>
        /// Start a downloading task.
        /// </summary>
        /// <param name="downloadTaskInfo">Downloading task info.</param>
        /// <returns>A unique ID of the downloading task.</returns>
        int StartDownloading(DownloadTaskInfo downloadTaskInfo);

        /// <summary>
        /// Stop a downloading task.
        /// </summary>
        /// <param name="taskId">Downloading task ID.</param>
        /// <param name="quiet">Stop this task without triggering any callback.</param>
        /// <returns>True if there is a downloading task with this ID.</returns>
        bool StopDownloading(int taskId, bool quiet = false);
    }
}

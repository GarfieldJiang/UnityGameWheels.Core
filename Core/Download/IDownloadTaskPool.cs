namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface of any download task pool.
    /// </summary>
    public interface IDownloadTaskPool
    {
        /// <summary>
        /// Reference pool module.
        /// </summary>
        IRefPoolModule RefPoolModule { get; set; }

        /// <summary>
        /// Download module this task is attached to.
        /// </summary>
        IDownloadModule DownloadModule { get; set; }

        /// <summary>
        /// Initialize.
        /// </summary>
        void Init();

        /// <summary>
        /// Shut down.
        /// </summary>
        void ShutDown();

        /// <summary>
        /// Acquire a download task to use.
        /// </summary>
        /// <returns>The download task.</returns>
        IDownloadTask Acquire();

        /// <summary>
        /// Release a download task to the pool.
        /// </summary>
        /// <param name="downloadTask">The download task.</param>
        void Release(IDownloadTask downloadTask);
    }
}
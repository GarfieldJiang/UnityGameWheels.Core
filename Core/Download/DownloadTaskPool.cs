namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Default implementation of <see cref="IDownloadTaskPool"/>.
    /// </summary>
    public class DownloadTaskPool : IDownloadTaskPool
    {
        private IRefPool<DownloadTask> m_DownloadTaskRawPool = null;

        /// <summary>
        /// Reference pool module.
        /// </summary>
        public IRefPoolService RefPoolService { get; set; }

        /// <summary>
        /// Download module this task is attached to.
        /// </summary>
        public IDownloadModule DownloadModule { get; set; }

        /// <summary>
        /// Acquire a download task to use.
        /// </summary>
        /// <returns>The download task.</returns>
        public IDownloadTask Acquire()
        {
            return m_DownloadTaskRawPool.Acquire();
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        public void Init()
        {
            m_DownloadTaskRawPool = RefPoolService.GetOrAdd<DownloadTask>();
            m_DownloadTaskRawPool.Capacity = DownloadModule.ConcurrentDownloadCountLimit;
            m_DownloadTaskRawPool.ApplyCapacity();
        }

        /// <summary>
        /// Release a download task to the pool.
        /// </summary>
        /// <param name="downloadTask">The download task.</param>
        public void Release(IDownloadTask downloadTask)
        {
            m_DownloadTaskRawPool.Release((DownloadTask)downloadTask);
        }

        /// <summary>
        /// Shut down.
        /// </summary>
        public void ShutDown()
        {
            m_DownloadTaskRawPool.Clear();
            m_DownloadTaskRawPool = null;
            RefPoolService = null;
            DownloadModule = null;
        }
    }
}
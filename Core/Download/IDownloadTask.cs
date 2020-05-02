namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface of a real download task.
    /// </summary>
    public interface IDownloadTask
    {
        /// <summary>
        /// Download module this task is attached to.
        /// </summary>
        IDownloadService DownloadService { get; set; }

        /// <summary>
        /// ID of the download task.
        /// </summary>
        int DownloadTaskId { get; set; }

        /// <summary>
        /// Download task info.
        /// </summary>
        DownloadTaskInfo? Info { get; set; }

        /// <summary>
        /// Downloaded size in bytes.
        /// </summary>
        long DownloadedSize { get; }

        /// <summary>
        /// Error code.
        /// </summary>
        DownloadErrorCode? ErrorCode { get; }

        /// <summary>
        /// Error message.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Whether the current task is done successfully.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// Time used in seconds.
        /// </summary>
        float TimeUsed { get; }

        /// <summary>
        /// Start the task.
        /// </summary>
        void Start();

        /// <summary>
        /// Generic tick method.
        /// </summary>
        /// <param name="timeStruct">Time struct.</param>
        void Update(TimeStruct timeStruct);

        /// <summary>
        /// Stop the task.
        /// </summary>
        void Stop();

        /// <summary>
        /// Reset the task.
        /// </summary>
        void Reset();

        /// <summary>
        /// Initialize.
        /// </summary>
        void Init();
    }
}
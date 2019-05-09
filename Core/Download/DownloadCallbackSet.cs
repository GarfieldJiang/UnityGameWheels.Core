namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Callback set for a download task.
    /// </summary>
    public struct DownloadCallbackSet
    {
        public OnDownloadFailure OnFailure;
        public OnDownloadProgress OnProgress;
        public OnDownloadSuccess OnSuccess;
    }

    /// <summary>
    /// Callback indicating that a downloading task has failed.
    /// </summary>
    /// <param name="downloadTaskId">Downloading task ID.</param>
    /// <param name="downloadTaskInfo">Downloading task information.</param>
    /// <param name="errorCode">Error code.</param>
    /// <param name="errorMessage">Error message.</param>
    public delegate void OnDownloadFailure(int downloadTaskId, DownloadTaskInfo downloadTaskInfo, DownloadErrorCode errorCode,
        string errorMessage);

    /// <summary>
    /// Callback indicating that a downloading task has new bytes downloaded.
    /// </summary>
    /// <param name="downloadTaskId">Downloading task ID.</param>
    /// <param name="downloadTaskInfo">Downloading task information.</param>
    /// <param name="downloadedSize">Downloaded size in bytes.</param>
    public delegate void OnDownloadProgress(int downloadTaskId, DownloadTaskInfo downloadTaskInfo, long downloadedSize);

    /// <summary>
    /// Callback indicating that a downloading task has finished successfully.
    /// </summary>
    /// <param name="downloadTaskId">Downloading task ID.</param>
    /// <param name="downloadTaskInfo">Downloading task information.</param>
    public delegate void OnDownloadSuccess(int downloadTaskId, DownloadTaskInfo downloadTaskInfo);
}
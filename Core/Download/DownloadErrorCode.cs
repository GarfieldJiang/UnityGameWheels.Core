namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Download error code.
    /// </summary>
    public enum DownloadErrorCode
    {
        /// <summary>
        /// We don't know why this error is raised.
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// The download task is stopped by the user.
        /// </summary>
        StoppedByUser,

        /// <summary>
        /// Exceeds the time limit.
        /// </summary>
        Timeout,

        /// <summary>
        /// Network error.
        /// </summary>
        Network,

        /// <summary>
        /// Web layer error.
        /// </summary>
        Web,

        /// <summary>
        /// The checksum of the downloaded content is wrong.
        /// </summary>
        WrongChecksum,

        /// <summary>
        /// The size of the downloaded content is wrong.
        /// </summary>
        WrongSize,

        /// <summary>
        /// File IO exception is raised.
        /// </summary>
        FileIOException,
    }
}

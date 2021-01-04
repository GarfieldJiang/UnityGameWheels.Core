namespace COL.UnityGameWheels.Core
{
    public interface IDownloadServiceConfigReader : IConfigReader
    {
        /// <summary>
        /// Temporary file extension, starting with a full stop.
        /// </summary>
        string TempFileExtension { get; }

        /// <summary>
        /// The upper limit of the number of concurrent downloading tasks.
        /// </summary>
        int ConcurrentDownloadCountLimit { get; }

        /// <summary>
        /// The chunk size in bytes to save to the disk. A value that is less than or equal to 0 means the download won't be chunk based.
        /// </summary>
        int ChunkSizeToSave { get; }

        /// <summary>
        /// Time limit for any task with no progress (in seconds).
        /// </summary>
        float Timeout { get; }
    }
}
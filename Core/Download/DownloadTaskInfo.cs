using System;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Downloading task information
    /// </summary>
    public struct DownloadTaskInfo
    {
        /// <summary>
        /// The URL string from which to download the file.
        /// </summary>
        public string UrlStr { get; private set; }

        /// <summary>
        /// Where the downloaded file will be saved.
        /// </summary>
        public string SavePath { get; private set; }

        /// <summary>
        /// Size in bytes of the resource to download.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// Gets the expected CRC32 checksum for the content to download.
        /// </summary>
        /// <value>Expected CRC32 checksum for the content to download.</value>
        public uint? Crc32 { get; private set; }

        /// <summary>
        /// Callback set.
        /// </summary>
        public DownloadCallbackSet CallbackSet { get; private set; }

        /// <summary>
        /// Context.
        /// </summary>
        public object Context { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="urlStr">The URL string from which to download the file.</param>
        /// <param name="savePath">Where the downloaded file will be saved.</param>
        /// <param name="callbackSet">Callback set.</param>
        public DownloadTaskInfo(string urlStr, string savePath, DownloadCallbackSet callbackSet)
            : this(urlStr, savePath, -1, null, callbackSet, null)
        {
            // Empty.
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="urlStr">The URL string from which to download the file.</param>
        /// <param name="savePath">Where the downloaded resource will be saved.</param>
        /// <param name="size">Size in bytes of the resource to download.</param>
        /// <param name="crc32">Expected CRC32 checksum for the content to download.</param>
        /// <param name="callbackSet">Callback set.</param>
        /// <param name="context">Context.</param>
        public DownloadTaskInfo(string urlStr, string savePath, long size, uint? crc32,
            DownloadCallbackSet callbackSet, object context)
        {
            if (string.IsNullOrEmpty(urlStr))
            {
                throw new ArgumentException("urlStr");
            }

            if (string.IsNullOrEmpty(savePath))
            {
                throw new ArgumentException("savePath");
            }

            UrlStr = urlStr;
            SavePath = savePath;
            Size = size;
            Crc32 = crc32;
            CallbackSet = callbackSet;
            Context = context;
        }
    }
}
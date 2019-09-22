namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Constant.
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// Index file's name.
        /// </summary>
        public const string IndexFileName = "index.dat";

        /// <summary>
        /// Name of cached remote index file.
        /// </summary>
        public const string CachedRemoteIndexFileName = "remote_index.dat";

        /// <summary>
        /// File header of the index file in the installer path.
        /// </summary>
        public const string InstallerIndexFileHeader = "CRI";

        /// <summary>
        /// File header of the index file in the read-write path.
        /// </summary>
        public const string ReadWriteIndexFileHeader = "PRI";

        /// <summary>
        /// File header of the remote index file.
        /// </summary>
        public const string RemoteIndexFileHeader = "RRI";

        /// <summary>
        /// The extension of any resource file.
        /// </summary>
        public const string ResourceFileExtension = ".dat";

        /// <summary>
        /// Invalid resource group ID.
        /// </summary>
        public const int InvalidResourceGroupId = -1;

        /// <summary>
        /// Common resource group ID.
        /// </summary>
        public const int CommonResourceGroupId = 0;
    }
}

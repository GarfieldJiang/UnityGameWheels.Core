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
        /// Name of cached, zipped remote index file.
        /// </summary>
        public const string CachedRemoteIndexZipFileName = "remote_index.zip";

        public const string InstallerIndexFileHeader_Obsolete = "CRI";

        public const string ReadWriteIndexFileHeader_Obsolete = "PRI";

        public const string RemoteIndexFileHeader_Obsolete = "RRI";

        /// <summary>
        /// File header of the index file in the installer path.
        /// </summary>
        public const string InstallerIndexFileHeader = "CR";

        /// <summary>
        /// File header of the index file in the read-write path.
        /// </summary>
        public const string ReadWriteIndexFileHeader = "PR";

        /// <summary>
        /// File header of the remote index file.
        /// </summary>
        public const string RemoteIndexFileHeader = "RR";

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
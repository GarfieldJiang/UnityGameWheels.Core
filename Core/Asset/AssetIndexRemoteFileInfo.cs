using System;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Remote index file information.
    /// </summary>
    public class AssetIndexRemoteFileInfo
    {
        /// <summary>
        /// Internal asset version.
        /// </summary>
        public int InternalAssetVersion { get; }

        /// <summary>
        /// CRC 32 checksum.
        /// </summary>
        public uint Crc32 { get; }

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="internalAssetVersion">Internal asset version.</param>
        /// <param name="crc32">CRC 32 checksum.</param>
        /// <param name="fileSize">File size in bytes.</param>
        public AssetIndexRemoteFileInfo(int internalAssetVersion, uint crc32, long fileSize)
        {
            if (internalAssetVersion <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(internalAssetVersion), "Must be positive.");
            }

            if (fileSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fileSize), "Must be positive.");
            }

            InternalAssetVersion = internalAssetVersion;
            Crc32 = crc32;
            FileSize = fileSize;
        }
    }
}
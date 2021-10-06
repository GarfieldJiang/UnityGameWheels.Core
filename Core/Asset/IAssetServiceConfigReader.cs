using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    public interface IAssetServiceConfigReader
    {
        string RunningPlatform { get; }
        bool UpdateIsEnabled { get; }
        int DownloadRetryCount { get; }
        int ConcurrentAssetLoaderCount { get; }
        int ConcurrentResourceLoaderCount { get; }
        int AssetCachePoolCapacity { get; }
        int ResourceCachePoolCapacity { get; }
        int AssetAccessorPoolCapacity { get; }
        string UpdateRelativePathFormat { get; }
        string ReadWritePath { get; }
        string InstallerPath { get; }
        IEnumerable<string> UpdateServerRootUrls { get; }
        float ReleaseResourceInterval { get; }
        int UpdateSizeBeforeSavingReadWriteIndex { get; }
    }
}
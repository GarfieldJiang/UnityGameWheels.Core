using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    public class AssetCacheQuery : BaseCacheQuery
    {
        public AssetCacheStatus Status { get; internal set; }

        internal HashSet<string> DependencyAssetPaths;

        public ICollection<string> GetDependencyAssetPaths()
        {
            return DependencyAssetPaths == null ? new HashSet<string>() : new HashSet<string>(DependencyAssetPaths);
        }

        public string ResourcePath { get; internal set; }
    }
}

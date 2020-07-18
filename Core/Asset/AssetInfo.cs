using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset info.
    /// </summary>
    public class AssetInfo
    {
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the resource path.
        /// </summary>
        /// <value>The resource path.</value>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Gets the dependency asset paths.
        /// </summary>
        /// <value>The dependency asset paths.</value>
        public HashSet<string> DependencyAssetPaths { get; } = new HashSet<string>();
    }
}
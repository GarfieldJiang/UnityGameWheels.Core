using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    public class ResourceBasicInfo
    {
        public string Path = string.Empty;

        public int GroupId = 0;

        /// <summary>
        /// Resource paths that depend on this resource.
        /// </summary>
        /// <remarks>This property exists for compatibility to earlier version of asset index files.</remarks>
        public HashSet<string> DependingResourcePaths { get; } = new HashSet<string>();

        /// <summary>
        /// Resource paths on which this resource depends.
        /// </summary>
        public HashSet<string> DependencyResourcePaths { get; } = new HashSet<string>();
    }
}
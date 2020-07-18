using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    public class ResourceGroupInfo
    {
        public int GroupId { get; set; }

        public HashSet<string> ResourcePaths { get; } = new HashSet<string>();
    }
}
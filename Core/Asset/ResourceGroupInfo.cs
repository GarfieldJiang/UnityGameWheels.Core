using System.Collections.Generic;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public class ResourceGroupInfo : IBinarySerializable
    {
        public int GroupId { get; set; }

        public HashSet<string> ResourcePaths { get; } = new HashSet<string>();

        public void FromBinary(BinaryReader br)
        {
            GroupId = br.ReadInt32();
            var resourcePathCount = br.ReadInt32();
            ResourcePaths.Clear();
            for (int i = 0; i < resourcePathCount; i++)
            {
                ResourcePaths.Add(br.ReadString());
            }
        }

        public void ToBinary(BinaryWriter bw)
        {
            bw.Write(GroupId);
            bw.Write(ResourcePaths.Count);
            foreach (var resourcePath in ResourcePaths)
            {
                bw.Write(resourcePath);
            }
        }
    }
}

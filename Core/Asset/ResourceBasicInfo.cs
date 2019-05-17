using System.Collections.Generic;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public class ResourceBasicInfo : IBinarySerializable
    {
        public string Path = string.Empty;

        public int GroupId = 0;

        public HashSet<string> DependingResourcePaths { get; } = new HashSet<string>();

        public void FromBinary(BinaryReader br)
        {
            Path = br.ReadString();
            GroupId = br.ReadInt32();
            DependingResourcePaths.Clear();
            var dependingResourcePathCount = br.ReadInt32();
            for (int i = 0; i < dependingResourcePathCount; i++)
            {
                DependingResourcePaths.Add(br.ReadString());
            }
        }

        public void ToBinary(BinaryWriter bw)
        {
            bw.Write(Path);
            bw.Write(GroupId);
            bw.Write(DependingResourcePaths.Count);
            foreach (var dependingResourcePath in DependingResourcePaths)
            {
                bw.Write(dependingResourcePath);
            }
        }
    }
}
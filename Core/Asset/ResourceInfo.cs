using System.Collections.Generic;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public class ResourceInfo : IBinarySerializable
    {
        public string Path = string.Empty;

        public uint Crc32 = 0;

        public long Size = 0L;

        public string Hash = string.Empty;

        public HashSet<string> DependingResourcePaths { get; } = new HashSet<string>();

        public virtual void ToBinary(BinaryWriter bw)
        {
            bw.Write(Path);
            bw.Write(Crc32);
            bw.Write(Size);
            bw.Write(Hash);
            bw.Write(DependingResourcePaths.Count);
            foreach (var dependingResourcePath in DependingResourcePaths)
            {
                bw.Write(dependingResourcePath);
            }
        }

        public virtual void FromBinary(BinaryReader br)
        {
            Path = br.ReadString();
            Crc32 = br.ReadUInt32();
            Size = br.ReadInt64();
            Hash = br.ReadString();
            DependingResourcePaths.Clear();
            var dependingResourcePathCount = br.ReadInt32();
            for (int i = 0; i < dependingResourcePathCount; i++)
            {
                DependingResourcePaths.Add(br.ReadString());
            }
        }
    }
}
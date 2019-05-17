using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public class ResourceInfo : IBinarySerializable
    {
        public string Path = string.Empty;

        public uint Crc32 = 0;

        public long Size = 0L;

        public string Hash = string.Empty;

        public virtual void ToBinary(BinaryWriter bw)
        {
            bw.Write(Path);
            bw.Write(Crc32);
            bw.Write(Size);
            bw.Write(Hash);
        }

        public virtual void FromBinary(BinaryReader br)
        {
            Path = br.ReadString();
            Crc32 = br.ReadUInt32();
            Size = br.ReadInt64();
            Hash = br.ReadString();
        }
    }
}
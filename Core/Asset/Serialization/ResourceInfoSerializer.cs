using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    internal class ResourceInfoSerializer : IBinarySerializer<ResourceInfo>
    {
        public void ToBinary(BinaryWriter bw, ResourceInfo obj)
        {
            bw.Write(obj.Path);
            bw.Write(obj.Crc32);
            bw.Write(obj.Size);
            bw.Write(obj.Hash);
        }

        public void FromBinary(BinaryReader br, ResourceInfo obj)
        {
            obj.Path = br.ReadString();
            obj.Crc32 = br.ReadUInt32();
            obj.Size = br.ReadInt64();
            obj.Hash = br.ReadString();
        }
    }
}
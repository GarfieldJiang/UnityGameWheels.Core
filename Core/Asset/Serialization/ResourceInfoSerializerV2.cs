using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    internal class ResourceInfoSerializerV2 : IBinarySerializer<ResourceInfo>
    {
        private readonly StringMap m_StringMap;

        internal ResourceInfoSerializerV2(StringMap stringMap)
        {
            m_StringMap = stringMap;
        }

        public void ToBinary(BinaryWriter bw, ResourceInfo obj)
        {
            bw.Write(m_StringMap.GetId(obj.Path));
            bw.Write(obj.Crc32);
            bw.Write(obj.Size);
            bw.Write(obj.Hash);
        }

        public void FromBinary(BinaryReader br, ResourceInfo obj)
        {
            obj.Path = m_StringMap.GetString(br.ReadInt32());
            obj.Crc32 = br.ReadUInt32();
            obj.Size = br.ReadInt64();
            obj.Hash = br.ReadString();
        }
    }
}
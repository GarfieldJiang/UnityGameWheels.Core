using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    internal class ResourceGroupInfoSerializerV2 : IBinarySerializer<ResourceGroupInfo>
    {
        private readonly StringMap m_StringMap;

        internal ResourceGroupInfoSerializerV2(StringMap stringMap)
        {
            m_StringMap = stringMap;
        }

        public void ToBinary(BinaryWriter bw, ResourceGroupInfo obj)
        {
            bw.Write(obj.GroupId);
            bw.Write(obj.ResourcePaths.Count);
            foreach (var resourcePath in obj.ResourcePaths)
            {
                bw.Write(m_StringMap.GetId(resourcePath));
            }
        }

        public void FromBinary(BinaryReader br, ResourceGroupInfo obj)
        {
            obj.GroupId = br.ReadInt32();
            var resourcePathCount = br.ReadInt32();
            obj.ResourcePaths.Clear();
            for (int i = 0; i < resourcePathCount; i++)
            {
                obj.ResourcePaths.Add(m_StringMap.GetString(br.ReadInt32()));
            }
        }
    }
}
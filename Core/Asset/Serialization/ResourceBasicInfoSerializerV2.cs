using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    internal class ResourceBasicInfoSerializerV2 : IBinarySerializer<ResourceBasicInfo>
    {
        private readonly StringMap m_StringMap;

        internal ResourceBasicInfoSerializerV2(StringMap stringMap)
        {
            m_StringMap = stringMap;
        }

        public void ToBinary(BinaryWriter bw, ResourceBasicInfo obj)
        {
            bw.Write(m_StringMap.GetId(obj.Path));
            bw.Write(obj.GroupId);
            bw.Write(obj.DependencyResourcePaths.Count);
            foreach (var dependency in obj.DependencyResourcePaths)
            {
                bw.Write(m_StringMap.GetId(dependency));
            }
        }

        public void FromBinary(BinaryReader br, ResourceBasicInfo obj)
        {
            obj.Path = m_StringMap.GetString(br.ReadInt32());
            obj.GroupId = br.ReadInt32();
            obj.DependencyResourcePaths.Clear();
            var dependencyCount = br.ReadInt32();
            for (int i = 0; i < dependencyCount; i++)
            {
                obj.DependencyResourcePaths.Add(m_StringMap.GetString(br.ReadInt32()));
            }
        }
    }
}
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    internal class AssetInfoSerializerV2 : IBinarySerializer<AssetInfo>
    {
        private readonly StringMap m_StringMap;

        public AssetInfoSerializerV2(StringMap stringMap)
        {
            m_StringMap = stringMap;
        }

        public void ToBinary(BinaryWriter bw, AssetInfo obj)
        {
            bw.Write(m_StringMap.GetId(obj.Path));
            bw.Write(m_StringMap.GetId(obj.ResourcePath));
            bw.Write(obj.DependencyAssetPaths.Count);
            foreach (var path in obj.DependencyAssetPaths)
            {
                bw.Write(m_StringMap.GetId(path));
            }
        }

        public void FromBinary(BinaryReader br, AssetInfo obj)
        {
            obj.Path = m_StringMap.GetString(br.ReadInt32());
            obj.ResourcePath = m_StringMap.GetString(br.ReadInt32());
            obj.DependencyAssetPaths.Clear();
            var dependencyAssetPathCount = br.ReadInt32();
            for (var i = 0; i < dependencyAssetPathCount; i++)
            {
                obj.DependencyAssetPaths.Add(m_StringMap.GetString(br.ReadInt32()));
            }
        }
    }
}
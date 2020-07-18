using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    internal class AssetInfoSerializer : IBinarySerializer<AssetInfo>
    {

        public void ToBinary(BinaryWriter bw, AssetInfo obj)
        {
            bw.Write(obj.Path);
            bw.Write(obj.ResourcePath);
            bw.Write(obj.DependencyAssetPaths.Count);
            foreach (var path in obj.DependencyAssetPaths)
            {
                bw.Write(path);
            }
        }

        public void FromBinary(BinaryReader br, AssetInfo obj)
        {
            obj.Path = br.ReadString();
            obj.ResourcePath = br.ReadString();

            obj.DependencyAssetPaths.Clear();
            var dependencyAssetPathCount = br.ReadInt32();
            for (int i = 0; i < dependencyAssetPathCount; i++)
            {
                obj.DependencyAssetPaths.Add(br.ReadString());
            }
        }
    }
}
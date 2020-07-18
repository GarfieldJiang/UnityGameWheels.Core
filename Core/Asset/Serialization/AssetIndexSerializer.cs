using System;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public class AssetIndexSerializer : IBinarySerializer<AssetIndexBase>
    {
        public void ToBinary(BinaryWriter bw, AssetIndexBase obj)
        {
            var resourceGroupInfoSerializer = new ResourceGroupInfoSerializer();
            var resourceBasicInfoSerializer = new ResourceBasicInfoSerializer();
            var assetInfoSerializer = new AssetInfoSerializer();
            var resourceInfoSerializer = new ResourceInfoSerializer();

            bw.Write(obj.ObsoleteHeader);

            obj.SerializeAugmentedData(bw);

            bw.Write(obj.ResourceGroupInfos.Count);
            foreach (var resourceGroupInfo in obj.ResourceGroupInfos)
            {
                resourceGroupInfoSerializer.ToBinary(bw, resourceGroupInfo);
            }

            bw.Write(obj.ResourceBasicInfos.Count);
            foreach (var kv in obj.ResourceBasicInfos)
            {
                resourceBasicInfoSerializer.ToBinary(bw, kv.Value);
            }

            bw.Write(obj.AssetInfos.Count);
            foreach (var kv in obj.AssetInfos)
            {
                assetInfoSerializer.ToBinary(bw, kv.Value);
            }

            bw.Write(obj.ResourceInfos.Count);
            foreach (var kv in obj.ResourceInfos)
            {
                resourceInfoSerializer.ToBinary(bw, kv.Value);
            }
        }

        public void FromBinary(BinaryReader br, AssetIndexBase obj)
        {
            obj.Clear();
            var resourceGroupInfoSerializer = new ResourceGroupInfoSerializer();
            var resourceBasicInfoSerializer = new ResourceBasicInfoSerializer();
            var assetInfoSerializer = new AssetInfoSerializer();
            var resourceInfoSerializer = new ResourceInfoSerializer();

            var header = br.ReadString();
            if (header != obj.ObsoleteHeader)
            {
                throw new InvalidOperationException(
                    Utility.Text.Format("Expected header is '{0}', but actual header is '{1}'.", obj.ObsoleteHeader, header));
            }

            obj.DeserializeAugmentedData(br);

            var resourceGroupInfoCount = br.ReadInt32();
            obj.ResourceGroupInfos.Clear();
            for (var i = 0; i < resourceGroupInfoCount; i++)
            {
                var resourceGroupInfo = new ResourceGroupInfo();
                resourceGroupInfoSerializer.FromBinary(br, resourceGroupInfo);
                obj.ResourceGroupInfos.Add(resourceGroupInfo);
            }

            var resourceBasicInfoCount = br.ReadInt32();
            obj.ResourceBasicInfos.Clear();
            for (var i = 0; i < resourceBasicInfoCount; i++)
            {
                var resourceBasicInfo = new ResourceBasicInfo();
                resourceBasicInfoSerializer.FromBinary(br, resourceBasicInfo);
                obj.ResourceBasicInfos.Add(resourceBasicInfo.Path, resourceBasicInfo);
            }

            var assetInfoCount = br.ReadInt32();
            obj.AssetInfos.Clear();
            for (var i = 0; i < assetInfoCount; i++)
            {
                var assetInfo = new AssetInfo();
                assetInfoSerializer.FromBinary(br, assetInfo);
                obj.AssetInfos.Add(assetInfo.Path, assetInfo);
            }

            var resourceInfoCount = br.ReadInt32();
            obj.ResourceInfos.Clear();
            for (var i = 0; i < resourceInfoCount; i++)
            {
                var resourceInfo = new ResourceInfo();
                resourceInfoSerializer.FromBinary(br, resourceInfo);
                obj.ResourceInfos.Add(resourceInfo.Path, resourceInfo);
            }
        }
    }
}
using System;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public class AssetIndexSerializerV2 : IBinarySerializer<AssetIndexBase>
    {
        public const short Version = 2;

        public void ToBinary(BinaryWriter bw, AssetIndexBase obj)
        {
            var stringMap = new StringMap();
            var resourceGroupInfoSerializer = new ResourceGroupInfoSerializerV2(stringMap);
            var resourceBasicInfoSerializer = new ResourceBasicInfoSerializerV2(stringMap);
            var assetInfoSerializer = new AssetInfoSerializerV2(stringMap);
            var resourceInfoSerializer = new ResourceInfoSerializerV2(stringMap);

            bw.Write(obj.Header);
            bw.Write(Version);

            obj.SerializeAugmentedData(bw);

            foreach (var kv in obj.ResourceBasicInfos)
            {
                stringMap.TryAddString(kv.Key, out _);
            }

            foreach (var kv in obj.AssetInfos)
            {
                stringMap.TryAddString(kv.Key, out _);
            }

            stringMap.ToBinary(bw);

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
            var stringMap = new StringMap();
            var resourceGroupInfoSerializer = new ResourceGroupInfoSerializerV2(stringMap);
            var resourceBasicInfoSerializer = new ResourceBasicInfoSerializerV2(stringMap);
            var assetInfoSerializer = new AssetInfoSerializerV2(stringMap);
            var resourceInfoSerializer = new ResourceInfoSerializerV2(stringMap);

            var header = br.ReadString();
            if (header != obj.Header)
            {
                throw new InvalidOperationException(
                    Utility.Text.Format("Expected header is '{0}', but actual header is '{1}'.", obj.Header, header));
            }

            var version = br.ReadInt16();
            if (version != Version)
            {
                throw new AssetIndexWrongVersionException(Version, version);
            }

            obj.DeserializeAugmentedData(br);

            stringMap.FromBinary(br);

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
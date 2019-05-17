using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Base class of the asset index.
    /// </summary>
    public abstract class AssetIndexBase : IBinarySerializable
    {
        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>The header.</value>
        public abstract string Header { get; }


        private readonly Dictionary<string, ResourceInfo> m_ResourceInfos = new Dictionary<string, ResourceInfo>();

        /// <summary>
        /// Gets the resource infos.
        /// </summary>
        /// <value>The resource infos.</value>
        public IDictionary<string, ResourceInfo> ResourceInfos => m_ResourceInfos;

        private readonly Dictionary<string, AssetInfo> m_AssetInfos = new Dictionary<string, AssetInfo>();

        /// <summary>
        /// Gets the asset infos.
        /// </summary>
        /// <value>The asset infos.</value>
        public IDictionary<string, AssetInfo> AssetInfos => m_AssetInfos;

        private readonly List<ResourceGroupInfo> m_ResourceGroupInfos = new List<ResourceGroupInfo>();

        /// <summary>
        /// Gets the resource group infos.
        /// </summary>
        /// <value>The resource group infos.</value>
        public IList<ResourceGroupInfo> ResourceGroupInfos => m_ResourceGroupInfos;

        private readonly Dictionary<string, ResourceBasicInfo> m_ResourceBasicInfos = new Dictionary<string, ResourceBasicInfo>();


        /// <summary>
        /// Gets the resource basic infos
        /// </summary>
        public IDictionary<string, ResourceBasicInfo> ResourceBasicInfos => m_ResourceBasicInfos;

        protected abstract void SerializeAugmentedData(BinaryWriter bw);

        protected abstract void DeserializeAugmentedData(BinaryReader br);

        /// <summary>
        /// Writes to binary.
        /// </summary>
        /// <param name="bw">Binary writer.</param>
        public void ToBinary(BinaryWriter bw)
        {
            bw.Write(Header);

            SerializeAugmentedData(bw);

            bw.Write(ResourceGroupInfos.Count);
            foreach (var resourceGroupInfo in ResourceGroupInfos)
            {
                resourceGroupInfo.ToBinary(bw);
            }

            bw.Write(ResourceBasicInfos.Count);
            foreach (var kv in ResourceBasicInfos)
            {
                kv.Value.ToBinary(bw);
            }

            bw.Write(AssetInfos.Count);
            foreach (var kv in AssetInfos)
            {
                kv.Value.ToBinary(bw);
            }

            bw.Write(ResourceInfos.Count);
            foreach (var kv in ResourceInfos)
            {
                kv.Value.ToBinary(bw);
            }
        }

        /// <summary>
        /// Reads from binary.
        /// </summary>
        /// <param name="br">Binary reader.</param>
        public void FromBinary(BinaryReader br)
        {
            var header = br.ReadString();
            if (header != Header)
            {
                throw new InvalidOperationException(
                    Utility.Text.Format("Expected header is '{0}', but actual header is '{1}'.", Header, header));
            }

            DeserializeAugmentedData(br);

            int resourceGroupInfoCount = br.ReadInt32();
            m_ResourceGroupInfos.Clear();
            for (int i = 0; i < resourceGroupInfoCount; i++)
            {
                var resourceGroupInfo = new ResourceGroupInfo();
                resourceGroupInfo.FromBinary(br);
                m_ResourceGroupInfos.Add(resourceGroupInfo);
            }

            int resourceBasicInfoCount = br.ReadInt32();
            m_ResourceBasicInfos.Clear();
            for (int i = 0; i < resourceBasicInfoCount; i++)
            {
                var resourceBasicInfo = new ResourceBasicInfo();
                resourceBasicInfo.FromBinary(br);
                m_ResourceBasicInfos.Add(resourceBasicInfo.Path, resourceBasicInfo);
            }

            int assetInfoCount = br.ReadInt32();
            m_AssetInfos.Clear();
            for (int i = 0; i < assetInfoCount; i++)
            {
                var assetInfo = new AssetInfo();
                assetInfo.FromBinary(br);
                m_AssetInfos.Add(assetInfo.Path, assetInfo);
            }

            int resourceInfoCount = br.ReadInt32();
            m_ResourceInfos.Clear();
            for (int i = 0; i < resourceInfoCount; i++)
            {
                var resourceInfo = new ResourceInfo();
                resourceInfo.FromBinary(br);
                m_ResourceInfos.Add(resourceInfo.Path, resourceInfo);
            }
        }

        public virtual void Clear()
        {
            m_ResourceInfos.Clear();
            m_AssetInfos.Clear();
            m_ResourceBasicInfos.Clear();
            m_ResourceGroupInfos.Clear();
        }
    }
}
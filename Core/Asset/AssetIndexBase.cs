using System.Collections.Generic;
using System.Data;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Base class of the asset index.
    /// </summary>
    public abstract class AssetIndexBase
    {
        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>The header.</value>
        public abstract string Header { get; }

        public abstract string ObsoleteHeader { get; }


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

        private readonly Dictionary<string, ResourceBasicInfo> m_ResourceBasicInfos =
            new Dictionary<string, ResourceBasicInfo>();


        /// <summary>
        /// Gets the resource basic infos
        /// </summary>
        public IDictionary<string, ResourceBasicInfo> ResourceBasicInfos => m_ResourceBasicInfos;

        protected internal abstract void SerializeAugmentedData(BinaryWriter bw);

        protected internal abstract void DeserializeAugmentedData(BinaryReader br);

        public virtual void Clear()
        {
            m_ResourceInfos.Clear();
            m_AssetInfos.Clear();
            m_ResourceBasicInfos.Clear();
            m_ResourceGroupInfos.Clear();
        }
    }
}
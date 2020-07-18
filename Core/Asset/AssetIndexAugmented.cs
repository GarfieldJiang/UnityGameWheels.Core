using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Augmented base class of the asset index stored in the client installer path and the update server.
    /// </summary>
    public abstract class AssetIndexAugmented : AssetIndexBase
    {
        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value>The platform.</value>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bundle version.
        /// </summary>
        /// <value>The bundle version.</value>
        public string BundleVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the internal asset version.
        /// </summary>
        /// <value>The internal asset version.</value>
        public int InternalAssetVersion { get; set; } = 0;

        protected internal override void SerializeAugmentedData(BinaryWriter bw)
        {
            bw.Write(Platform);
            bw.Write(BundleVersion);
            bw.Write(InternalAssetVersion);
        }

        protected internal override void DeserializeAugmentedData(BinaryReader br)
        {
            Platform = br.ReadString();
            BundleVersion = br.ReadString();
            InternalAssetVersion = br.ReadInt32();
        }

        public override void Clear()
        {
            base.Clear();
            Platform = string.Empty;
            BundleVersion = string.Empty;
            InternalAssetVersion = 0;
        }
    }
}
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset index in the read-write path.
    /// </summary>
    public class AssetIndexForReadWrite : AssetIndexBase
    {
        public override string Header => Constant.ReadWriteIndexFileHeader;

        protected override void SerializeAugmentedData(BinaryWriter bw)
        {
            // Empty.
        }

        protected override void DeserializeAugmentedData(BinaryReader br)
        {
            // Empty.
        }
    }
}
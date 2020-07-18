using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset index in the read-write path.
    /// </summary>
    public class AssetIndexForReadWrite : AssetIndexBase
    {
        public override string Header => Constant.ReadWriteIndexFileHeader;

        public override string ObsoleteHeader => Constant.ReadWriteIndexFileHeader_Obsolete;

        protected internal override void SerializeAugmentedData(BinaryWriter bw)
        {
            // Empty.
        }

        protected internal override void DeserializeAugmentedData(BinaryReader br)
        {
            // Empty.
        }
    }
}
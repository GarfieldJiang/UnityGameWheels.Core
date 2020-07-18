namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset index in the remote server.
    /// </summary>
    public class AssetIndexForRemote : AssetIndexAugmented
    {
        public override string Header => Constant.RemoteIndexFileHeader;

        public override string ObsoleteHeader => Constant.RemoteIndexFileHeader_Obsolete;
    }
}
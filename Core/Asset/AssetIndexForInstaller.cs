namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset index in the installer path.
    /// </summary>
    public class AssetIndexForInstaller : AssetIndexAugmented
    {
        public override string Header => Constant.InstallerIndexFileHeader;
    }
}

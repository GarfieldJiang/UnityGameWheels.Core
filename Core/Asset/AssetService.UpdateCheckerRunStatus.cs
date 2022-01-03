namespace COL.UnityGameWheels.Core.Asset
{
    public partial class AssetService
    {
        private enum UpdateCheckerRunStatus
        {
            None,
            Waiting,
            CheckingNeedDownloadRemoteIndex,
            DownloadingRemoteIndex,
            UnzippingRemoteIndex,
        }
    }
}
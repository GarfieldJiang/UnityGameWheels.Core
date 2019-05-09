using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public delegate void OnLoadAssetIndexForInstallerSuccess(Stream stream, object context);

    public delegate void OnLoadAssetIndexForInstallerFailure(object context);

    public struct LoadAssetIndexForInstallerCallbackSet
    {
        public OnLoadAssetIndexForInstallerSuccess OnSuccess;
        public OnLoadAssetIndexForInstallerFailure OnFailure;
    }
}

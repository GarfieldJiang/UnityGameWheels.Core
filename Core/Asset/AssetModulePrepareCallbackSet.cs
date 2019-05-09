namespace COL.UnityGameWheels.Core.Asset
{
    public delegate void OnPrepareAssetModuleSuccess(object context);

    public delegate void OnPrepareAssetModuleFailure(string errorMessage, object context);

    public struct AssetModulePrepareCallbackSet
    {
        public OnPrepareAssetModuleSuccess OnSuccess;

        public OnPrepareAssetModuleFailure OnFailure;
    }
}

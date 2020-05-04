namespace COL.UnityGameWheels.Core.Asset
{
    public delegate void OnPrepareAssetServiceSuccess(object context);

    public delegate void OnPrepareAssetServiceFailure(string errorMessage, object context);

    public struct AssetServicePrepareCallbackSet
    {
        public OnPrepareAssetServiceSuccess OnSuccess;

        public OnPrepareAssetServiceFailure OnFailure;
    }
}
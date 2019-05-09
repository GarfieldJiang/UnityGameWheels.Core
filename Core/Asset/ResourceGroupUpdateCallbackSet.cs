namespace COL.UnityGameWheels.Core.Asset
{
    public delegate void OnUpdateResourceProgress(string resourcePath, long updatedSize, long totalSize, object context);

    public delegate void OnUpdateResourceSuccess(string resourcePath, long totalSize, object context);

    public delegate void OnUpdateResourceFailure(string resourcePath, string errorMessage, object context);

    public delegate void OnUpdateAllResourcesSuccess(object context);

    public delegate void OnUpdateAllResourcesFailure(string errorMessage, object context);

    public struct ResourceGroupUpdateCallbackSet
    {
        public OnUpdateResourceSuccess OnSingleSuccess;

        public OnUpdateResourceFailure OnSingleFailure;

        public OnUpdateResourceProgress OnSingleProgress;

        public OnUpdateAllResourcesFailure OnAllFailure;

        public OnUpdateAllResourcesSuccess OnAllSuccess;
    }
}

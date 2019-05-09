namespace COL.UnityGameWheels.Core.Asset
{
    public enum AssetCacheStatus
    {
        None,
        Ready,
        Failure,
        WaitingForResource,
        WaitingForDeps,
        WaitingForSlot,
        Loading,
    }
}
namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Interface of asset accessor.
    /// </summary>
    public interface IAssetAccessor
    {
        /// <summary>
        /// Asset path.
        /// </summary>
        string AssetPath { get; }

        /// <summary>
        /// Asset object.
        /// </summary>
        object AssetObject { get; }

        /// <summary>
        /// Status.
        /// </summary>
        AssetAccessorStatus Status { get; }
    }
}
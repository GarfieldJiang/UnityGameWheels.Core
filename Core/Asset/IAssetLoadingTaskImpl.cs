namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset loading task implementation interface.
    /// </summary>
    public interface IAssetLoadingTaskImpl : ITask
    {
        /// <summary>
        /// The resource object from which to load asset.
        /// </summary>
        object ResourceObject { get; set; }

        /// <summary>
        /// The asset path.
        /// </summary>
        string AssetPath { get; set; }

        /// <summary>
        /// The loaded asset object.
        /// </summary>
        object AssetObject { get; }
    }
}
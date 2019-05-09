namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Loader of the read-only asset index file.
    /// </summary>
    public interface IAssetIndexForInstallerLoader
    {
        /// <summary>
        /// Load the read-only asset index file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="callbackSet">Callback set.</param>
        /// <param name="context">Context.</param>
        void Load(string path, LoadAssetIndexForInstallerCallbackSet callbackSet, object context);
    }
}
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset module interface.
    /// </summary>
    public interface IAssetService : ITickableService
    {
        IAssetServiceConfigReader ConfigReader { get; }

        /// <summary>
        /// Application version.
        /// </summary>
        string BundleVersion { get; set; }

        /// <summary>
        /// Current running platform.
        /// </summary>
        string RunningPlatform { get; set; }

        /// <summary>
        /// Add root urls of update servers.
        /// </summary>
        /// <param name="updateServerRootUrl">Root urls of update servers.</param>
        void AddUpdateServerRootUrl(string updateServerRootUrl);

        /// <summary>
        /// Prepare.
        /// </summary>
        /// <param name="callbackSet">Callback set.</param>
        /// <param name="context">Context.</param>
        void Prepare(AssetServicePrepareCallbackSet callbackSet, object context);

        /// <summary>
        /// Check which resources need to be updated.
        /// </summary>
        /// <param name="remoteIndexFileInfo">Index file information on the update server.</param>
        /// <param name="callbackSet">Callback set.</param>
        /// <param name="context">Context.</param>
        void CheckUpdate(AssetIndexRemoteFileInfo remoteIndexFileInfo, UpdateCheckCallbackSet callbackSet, object context);

        /// <summary>
        /// Gets the resource updater instance.
        /// </summary>
        IResourceUpdater ResourceUpdater { get; }

        /// <summary>
        /// Load a given non-scene asset.
        /// </summary>
        /// <param name="assetPath">Asset path.</param>
        /// <param name="callbackSet">Callback set.</param>
        /// <param name="context">Context.</param>
        /// <returns></returns>
        IAssetAccessor LoadAsset(string assetPath, LoadAssetCallbackSet callbackSet, object context);

        /// <summary>
        /// Load a given scene asset.
        /// </summary>
        /// <param name="sceneAssetPath">Scene asset path.</param>
        /// <param name="callbackSet">Callback set.</param>
        /// <param name="context">Context.</param>
        IAssetAccessor LoadSceneAsset(string sceneAssetPath, LoadAssetCallbackSet callbackSet, object context);

        /// <summary>
        /// Get the resource group ID which the given asset belongs.
        /// </summary>
        /// <param name="assetPath">Asset path.</param>
        /// <returns>Resource group ID, or <see cref="Constant.InvalidResourceGroupId"/> if asset doesn't exist.</returns>
        int GetAssetResourceGroupId(string assetPath);

        /// <summary>
        /// Reduces a reference to the given asset.
        /// </summary>
        /// <param name="assetAccessor">The asset accessor.</param>
        void UnloadAsset(IAssetAccessor assetAccessor);

        /// <summary>
        /// Whether any asset is being loaded.
        /// </summary>
        bool IsLoadingAnyAsset { get; }

        /// <summary>
        /// Requests to unload all unused resources.
        /// </summary>
        void RequestUnloadUnusedResources();

        /// <summary>
        /// Get all asset cache queries.
        /// </summary>
        /// <returns></returns>
        /// <remarks>For debug use.</remarks>
        IDictionary<string, AssetCacheQuery> GetAssetCacheQueries();

        /// <summary>
        /// Get all resource cache queries.
        /// </summary>
        /// <returns></returns>
        /// <remarks>For debug use.</remarks>
        IDictionary<string, ResourceCacheQuery> GetResourceCacheQueries();
    }
}
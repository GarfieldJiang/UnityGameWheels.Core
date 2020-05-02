using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset module interface.
    /// </summary>
    public interface IAssetModule : IModule
    {
        /// <summary>
        /// Download module.
        /// </summary>
        IDownloadModule DownloadModule { get; set; }

        /// <summary>
        /// Reference pool module.
        /// </summary>
        IRefPoolService RefPoolService { get; set; }

        /// <summary>
        /// This factory creates instances of <see cref="IAssetLoadingTaskImpl"/>.
        /// </summary>
        ISimpleFactory<IAssetLoadingTaskImpl> AssetLoadingTaskImplFactory { get; set; }

        /// <summary>
        /// This factory creates instances of <see cref="IResourceLoadingTaskImpl"/>.
        /// </summary>
        ISimpleFactory<IResourceLoadingTaskImpl> ResourceLoadingTaskImplFactory { get; set; }

        /// <summary>
        /// The implementation of how a resource object should be destroyed/unloaded.
        /// </summary>
        IObjectDestroyer<object> ResourceDestroyer { get; set; }

        /// <summary>
        /// How many <see cref="IAssetLoadingTaskImpl"/> instances can be run concurrently.
        /// </summary>
        int ConcurrentAssetLoaderCount { get; set; }

        /// <summary>
        /// How many <see cref="IResourceLoadingTaskImpl"/> instances can be run concurrently.
        /// </summary>
        int ConcurrentResourceLoaderCount { get; set; }

        /// <summary>
        /// How many asset caches to preserve at most.
        /// </summary>
        int AssetCachePoolCapacity { get; set; }

        /// <summary>
        /// How many resource caches to preserve at most.
        /// </summary>
        int ResourceCachePoolCapacity { get; set; }

        /// <summary>
        /// How many asset accessor to preserve at most.
        /// </summary>
        int AssetAccessorPoolCapacity { get; set; }

        /// <summary>
        /// How many times we should retry after the first time download fails.
        /// </summary>
        int DownloadRetryCount { get; set; }

        /// <summary>
        /// Time interval to check and release resources that are not retained.
        /// </summary>
        float ReleaseResourceInterval { get; set; }

        IAssetIndexForInstallerLoader IndexForInstallerLoader { get; set; }

        /// <summary>
        /// Whether resource update is enabled.
        /// </summary>
        bool UpdateIsEnabled { get; set; }

        /// <summary>
        /// Format of relative path to the update server.
        /// </summary>
        /// <remarks>Two arguments needed. {0} stands for the running platform and {1} stands for the asset version.</remarks>
        string UpdateRelativePathFormat { get; set; }

        /// <summary>
        /// Application version.
        /// </summary>
        string BundleVersion { get; set; }

        /// <summary>
        /// Path to the persistent resources path.
        /// </summary>
        string ReadWritePath { get; set; }

        /// <summary>
        /// Path to resources in the installation package.
        /// </summary>
        string InstallerPath { get; set; }

        /// <summary>
        /// Current running platform.
        /// </summary>
        string RunningPlatform { get; set; }

        /// <summary>
        /// Bytes to update before triggering a saving operation of the read-write index.
        /// </summary>
        /// <remarks>
        /// If this value is set to 0, then during updating, the read-write index will be saved to disk every time a resource file is
        /// successfully updated.
        /// </remarks>
        int UpdateSizeBeforeSavingReadWriteIndex { get; set; }

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
        void Prepare(AssetModulePrepareCallbackSet callbackSet, object context);

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
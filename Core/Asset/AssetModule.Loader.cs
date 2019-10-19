namespace COL.UnityGameWheels.Core.Asset
{
    using System;
    using System.Collections.Generic;

    public partial class AssetModule
    {
        internal sealed partial class Loader
        {
            private readonly AssetModule m_Owner = null;
            private readonly Dictionary<string, AssetCache> m_AssetCaches = new Dictionary<string, AssetCache>();
            private readonly Dictionary<string, ResourceCache> m_ResourceCaches = new Dictionary<string, ResourceCache>();
            private readonly HashSet<string> m_AssetPathsNotReadyOrFailure = new HashSet<string>();
            private readonly HashSet<string> m_ResourcePathsNotReadyOrFailure = new HashSet<string>();
            private readonly HashSet<AssetCache> m_UnretainedAssetCaches = new HashSet<AssetCache>();
            private readonly List<AssetCache> m_TempAssetCaches = new List<AssetCache>();
            private readonly HashSet<ResourceCache> m_UnretainedResourceCaches = new HashSet<ResourceCache>();
            private readonly List<Action<TimeStruct>> m_TickDelegates = new List<Action<TimeStruct>>();
            private readonly List<Action<TimeStruct>> m_TempTickDelegates = new List<Action<TimeStruct>>();
            private readonly List<IAssetLoadingTaskImpl> m_RunningAssetLoadingTasks = null;
            private readonly List<IResourceLoadingTaskImpl> m_RunningResourceLoadingTasks = null;
            private readonly List<AssetAccessor> m_AssetAccessorsToRelease = null;

            private AssetIndexForInstaller InstallerIndex => m_Owner.m_InstallerIndex;

            private AssetIndexForReadWrite ReadWriteIndex => m_Owner.m_ReadWriteIndex;

            private Dictionary<int, ResourceGroupUpdateSummary> ResourceSummaries => m_Owner.ResourceGroupUpdateSummaries;

            private ISimpleFactory<IAssetLoadingTaskImpl> AssetLoadingTaskImplFactory => m_Owner.AssetLoadingTaskImplFactory;

            private ISimpleFactory<IResourceLoadingTaskImpl> ResourceLoadingTaskImplFactory => m_Owner.ResourceLoadingTaskImplFactory;

            private int ConcurrentAssetLoaderCount => m_Owner.ConcurrentAssetLoaderCount;

            private int ConcurrentResourceLoaderCount => m_Owner.ConcurrentResourceLoaderCount;

            private string ReadWritePath => m_Owner.ReadWritePath;

            private string InstallerPath => m_Owner.InstallerPath;

            private IObjectDestroyer<object> ResourceDestroyer => m_Owner.ResourceDestroyer;

            private readonly IRefPool<AssetCache> m_AssetCachePool = null;
            private readonly IRefPool<ResourceCache> m_ResourceCachePool = null;
            private readonly IRefPool<AssetAccessor> m_AssetAccessorPool = null;
            private readonly IRefPool<AssetLoadingTask> m_AssetLoadingTaskPool = null;
            private readonly IRefPool<ResourceLoadingTask> m_ResourceLoadingTaskPool = null;

            private float m_LastReleaseResourcesTime = 0;

            //private readonly HashSet<string> m_DFSVisitedFlags = null;
            private bool m_ShouldForceUnloadUnusedResources;

            private readonly Dictionary<string, int> m_AssetPathToResourceGroupIdMap = new Dictionary<string, int>();

            internal Loader(AssetModule owner)
            {
                m_Owner = owner;
                m_RunningAssetLoadingTasks =
                    new List<IAssetLoadingTaskImpl>(ConcurrentAssetLoaderCount > 0 ? ConcurrentAssetLoaderCount : 16);
                m_RunningResourceLoadingTasks =
                    new List<IResourceLoadingTaskImpl>(ConcurrentResourceLoaderCount > 0 ? ConcurrentResourceLoaderCount : 8);
                m_AssetCachePool = m_Owner.RefPoolModule.Add<AssetCache>(owner.AssetCachePoolCapacity);
                m_ResourceCachePool = m_Owner.RefPoolModule.Add<ResourceCache>(owner.ResourceCachePoolCapacity);
                m_AssetAccessorPool = m_Owner.RefPoolModule.Add<AssetAccessor>(m_Owner.AssetAccessorPoolCapacity);
                m_AssetLoadingTaskPool = m_Owner.RefPoolModule.Add<AssetLoadingTask>(m_RunningAssetLoadingTasks.Capacity);
                m_ResourceLoadingTaskPool = m_Owner.RefPoolModule.Add<ResourceLoadingTask>(m_RunningResourceLoadingTasks.Capacity);
                //m_DFSVisitedFlags = new HashSet<string>();
                m_AssetAccessorsToRelease = new List<AssetAccessor>(m_Owner.AssetAccessorPoolCapacity / 8);
            }

            internal bool IsLoadingAnyAsset => m_AssetPathsNotReadyOrFailure.Count > 0 || m_ResourcePathsNotReadyOrFailure.Count > 0;

            internal void Update(TimeStruct timeStruct)
            {
                ReleaseAssetAccessors();
                UpdateRunningAssetLoadingTasks(timeStruct);
                UpdateRunningResourceLoadingTasks(timeStruct);
                UpdateTickDelegates(timeStruct);
                ReleaseUnretainedAssetCaches();

                if (m_ShouldForceUnloadUnusedResources ||
                    timeStruct.UnscaledTime - m_LastReleaseResourcesTime > m_Owner.ReleaseResourceInterval)
                {
                    m_LastReleaseResourcesTime = timeStruct.UnscaledTime;
                    m_ShouldForceUnloadUnusedResources = false;
                    ReleaseUnusedResourceCaches();
                }
            }

            private void ReleaseUnusedResourceCaches()
            {
                if (m_UnretainedResourceCaches.Count <= 0)
                {
                    return;
                }

                CoreLog.Debug($"Unretained count: {m_UnretainedResourceCaches.Count}");
                foreach (var resourceCache in m_UnretainedResourceCaches)
                {
                    m_ResourceCaches.Remove(resourceCache.Path);
                    resourceCache.Reset();
                    m_ResourceCachePool.Release(resourceCache);
                }

                m_UnretainedResourceCaches.Clear();
            }

            private void ReleaseUnretainedAssetCaches()
            {
                while (m_UnretainedAssetCaches.Count != 0)
                {
                    m_TempAssetCaches.Clear();
                    m_TempAssetCaches.AddRange(m_UnretainedAssetCaches);
                    foreach (var assetCache in m_TempAssetCaches)
                    {
                        m_AssetCaches.Remove(assetCache.Path);
                        m_UnretainedAssetCaches.Remove(assetCache);
                        assetCache.Reset();
                        m_AssetCachePool.Release(assetCache);
                    }

                    m_TempAssetCaches.Clear();
                }
            }

            private void UpdateTickDelegates(TimeStruct timeStruct)
            {
                m_TempTickDelegates.Clear();
                m_TempTickDelegates.AddRange(m_TickDelegates);
                foreach (var dele in m_TempTickDelegates)
                {
                    dele(timeStruct);
                }

                m_TempTickDelegates.Clear();
            }

            private void UpdateRunningResourceLoadingTasks(TimeStruct timeStruct)
            {
                foreach (var resourceLoadingTask in m_RunningResourceLoadingTasks)
                {
                    resourceLoadingTask.OnUpdate(timeStruct);
                }
            }

            private void UpdateRunningAssetLoadingTasks(TimeStruct timeStruct)
            {
                foreach (var assetLoadingTask in m_RunningAssetLoadingTasks)
                {
                    assetLoadingTask.OnUpdate(timeStruct);
                }
            }

            internal AssetAccessor LoadAsset(string assetPath, bool isScene, LoadAssetCallbackSet callbackSet, object context)
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    throw new ArgumentException("Shouldn't be null or empty.", "assetPath");
                }

                if (!ReadWriteIndex.AssetInfos.TryGetValue(assetPath, out var assetInfo))
                {
                    throw new ArgumentException(Utility.Text.Format("Asset info for path '{0}' not found.", assetPath));
                }

                int resourceGroup = ReadWriteIndex.ResourceBasicInfos[assetInfo.ResourcePath].GroupId;
                if (m_Owner.ResourceUpdater.GetResourceGroupStatus(resourceGroup) != ResourceGroupStatus.UpToDate)
                {
                    throw new InvalidOperationException(Utility.Text.Format(
                        "Asset '{0}' cannot be used until resource group '{1}' is done updating.",
                        assetPath, resourceGroup));
                }

                if (!m_AssetCaches.TryGetValue(assetPath, out var assetCache))
                {
                    assetCache = AcquireAssetCache(assetPath, assetInfo, isScene);
                }

                var ret = m_AssetAccessorPool.Acquire();
                ret.Init(assetCache, callbackSet, context);
                return ret;
            }

            internal void UnloadAsset(AssetAccessor assetAccessor)
            {
                if (!ReadWriteIndex.AssetInfos.TryGetValue(assetAccessor.AssetPath, out var assetInfo))
                {
                    var errorMessage = Utility.Text.Format("Asset info for path '{0}' not found.", assetAccessor.AssetPath);
                    throw new InvalidOperationException(errorMessage);
                }

                int resourceGroup = ReadWriteIndex.ResourceBasicInfos[assetInfo.ResourcePath].GroupId;
                if (m_Owner.ResourceUpdater.GetResourceGroupStatus(resourceGroup) != ResourceGroupStatus.UpToDate)
                {
                    throw new InvalidOperationException(Utility.Text.Format(
                        "Asset '{0}' cannot be used until resource group '{1}' is done updating.",
                        assetAccessor.AssetPath, resourceGroup));
                }

                m_AssetAccessorsToRelease.Add(assetAccessor);
            }

            internal int GetAssetResourceGroupId(string assetPath)
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    throw new ArgumentException("Shouldn't be null or empty.", nameof(assetPath));
                }

                if (!ReadWriteIndex.AssetInfos.TryGetValue(assetPath, out var assetInfo))
                {
                    return Constant.InvalidResourceGroupId;
                }

                if (m_AssetPathToResourceGroupIdMap.TryGetValue(assetPath, out int resourceGroupId))
                {
                    return resourceGroupId;
                }

                resourceGroupId = ReadWriteIndex.ResourceBasicInfos[assetInfo.ResourcePath].GroupId;
                m_AssetPathToResourceGroupIdMap.Add(assetPath, resourceGroupId);
                return resourceGroupId;
            }

            private AssetCache EnsureAssetCache(string assetPath)
            {
                if (m_AssetCaches.TryGetValue(assetPath, out var assetCache))
                {
                    return assetCache;
                }

                if (!ReadWriteIndex.AssetInfos.TryGetValue(assetPath, out var assetInfo))
                {
                    throw new InvalidOperationException(Utility.Text.Format("Asset info for path '{0}' not found.", assetPath));
                }

                // It must be guaranteed that scenes are not relied on by other assets.
                assetCache = AcquireAssetCache(assetPath, assetInfo, false);

                return assetCache;
            }

            private ResourceInfo EnsureResourceInfo(string resourcePath)
            {
                if (ReadWriteIndex.ResourceInfos.TryGetValue(resourcePath, out var ret))
                {
                    return ret;
                }

                if (InstallerIndex.ResourceInfos.TryGetValue(resourcePath, out ret))
                {
                    return ret;
                }

                throw new InvalidOperationException(Utility.Text.Format("Resource info for path '{0}' not found.", resourcePath));
            }

            private ResourceCache EnsureResourceCache(string resourcePath)
            {
                if (m_ResourceCaches.TryGetValue(resourcePath, out var resourceCache))
                {
                    return resourceCache;
                }

                bool fromReadWritePath = false;
                if (ReadWriteIndex.ResourceInfos.TryGetValue(resourcePath, out _))
                {
                    fromReadWritePath = true;
                }
                else if (!InstallerIndex.ResourceInfos.TryGetValue(resourcePath, out _))
                {
                    throw new InvalidOperationException(Utility.Text.Format("Resource info for path '{0}' not found.", resourcePath));
                }

                return AcquireResourceCache(resourcePath, fromReadWritePath);
            }

            private ResourceCache AcquireResourceCache(string resourcePath, bool fromReadWritePath)
            {
                ResourceCache resourceCache = m_ResourceCachePool.Acquire();
                resourceCache.Path = resourcePath;
                resourceCache.ShouldLoadFromReadWritePath = fromReadWritePath;
                resourceCache.Owner = this;
                m_ResourceCaches[resourcePath] = resourceCache;
                resourceCache.Init();
                return resourceCache;
            }

            private AssetCache AcquireAssetCache(string assetPath, AssetInfo assetInfo, bool isScene)
            {
                AssetCache assetCache = m_AssetCachePool.Acquire();
                assetCache.Path = assetPath;
                assetCache.DependencyAssetPaths = assetInfo.DependencyAssetPaths;
                assetCache.ResourcePath = assetInfo.ResourcePath;
                assetCache.Owner = this;
                assetCache.IsScene = isScene;
                m_AssetCaches[assetCache.Path] = assetCache;
                assetCache.Init();
                return assetCache;
            }

            private ResourceLoadingTask RunResourceLoadingTask(string resourcePath, string resourceParentDir)
            {
                var task = m_ResourceLoadingTaskPool.Acquire();
                task.OnCreate(ResourceLoadingTaskImplFactory);
                task.ResourcePath = resourcePath;
                task.ResourceParentDir = resourceParentDir;
                task.OnStart();
                m_RunningResourceLoadingTasks.Add(task);

                return task;
            }

            private void StopAndResetResourceLoadingTask(ResourceLoadingTask task)
            {
                m_RunningResourceLoadingTasks.Remove(task);
                task.OnReset();
                m_ResourceLoadingTaskPool.Release(task);
            }

            private AssetLoadingTask RunAssetLoadingTask(string assetPath, object resourceObject)
            {
                var task = m_AssetLoadingTaskPool.Acquire();
                task.OnCreate(AssetLoadingTaskImplFactory);
                task.ResourceObject = resourceObject;
                task.AssetPath = assetPath;
                task.OnStart();
                m_RunningAssetLoadingTasks.Add(task);
                return task;
            }

            private void StopAndResetAssetLoadingTask(AssetLoadingTask task)
            {
                m_RunningAssetLoadingTasks.Remove(task);
                task.OnReset();
                m_AssetLoadingTaskPool.Release(task);
            }

            internal IDictionary<string, AssetCacheQuery> GetAssetCacheQueries()
            {
                var ret = new Dictionary<string, AssetCacheQuery>();
                foreach (var kv in m_AssetCaches)
                {
                    var assetCache = kv.Value;
                    ret[kv.Key] = new AssetCacheQuery
                    {
                        DependencyAssetPaths = assetCache.DependencyAssetPaths,
                        ErrorMessage = assetCache.ErrorMessage,
                        LoadingProgress = assetCache.LoadingProgress,
                        Path = assetCache.Path,
                        ResourcePath = assetCache.ResourcePath,
                        RetainCount = assetCache.RetainCount,
                        Status = assetCache.Status,
                    };
                }

                return ret;
            }

            internal IDictionary<string, ResourceCacheQuery> GetResourceCacheQueries()
            {
                var ret = new Dictionary<string, ResourceCacheQuery>();
                foreach (var kv in m_ResourceCaches)
                {
                    var resourceCache = kv.Value;
                    ret[kv.Key] = new ResourceCacheQuery
                    {
                        ErrorMessage = resourceCache.ErrorMessage,
                        LoadingProgress = resourceCache.LoadingProgress,
                        Path = resourceCache.Path,
                        RetainCount = resourceCache.RetainCount,
                        Status = resourceCache.Status,
                    };
                }

                return ret;
            }

            internal void RequestUnloadUnusedResources()
            {
                m_ShouldForceUnloadUnusedResources = true;
            }

            private void ReleaseAssetAccessors()
            {
                if (m_AssetAccessorsToRelease.Count <= 0)
                {
                    return;
                }

                foreach (var assetAccessor in m_AssetAccessorsToRelease)
                {
                    assetAccessor.Reset();
                    m_AssetAccessorPool.Release(assetAccessor);
                }

                m_AssetAccessorsToRelease.Clear();
            }
        }
    }
}
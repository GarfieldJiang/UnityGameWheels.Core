using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    public partial class AssetModule
    {
        internal partial class Loader
        {
            internal class AssetCache : BaseCache
            {
                private static readonly HashSet<string> s_DFSVisitedFlag = new HashSet<string>();
                internal HashSet<string> DependencyAssetPaths = null;
                internal object AssetObject = null;
                private AssetLoadingTask m_LoadingTask = null;
                private readonly List<AssetCache> m_AssetObservers = new List<AssetCache>();
                private readonly List<AssetCache> m_CopiedAssetObservers = new List<AssetCache>();
                private readonly List<AssetAccessor> m_AssetAccessors = new List<AssetAccessor>();
                private readonly List<AssetAccessor> m_CopiedAssetAccessors = new List<AssetAccessor>();
                private readonly HashSet<string> m_DependencyResourcePaths = new HashSet<string>();

                private int m_DependencyAssetReadyCount = 0;

                public AssetCacheStatus Status { get; private set; }

                public string ResourcePath { get; internal set; }

                public override float LoadingProgress => m_LoadingTask?.Progress ?? 0f;

                private float m_LastLoadingProgress = 0;

                public bool IsScene { get; internal set; }

                public IEnumerable<string> GetDependencyAssetPaths()
                {
                    return DependencyAssetPaths == null ? new List<string>() : new List<string>(DependencyAssetPaths);
                }

                public AssetCache()
                {
                }

                internal override void Init()
                {
                    CoreLog.DebugFormat("[AssetCache Init] {0}", Path);
                    m_LastLoadingProgress = 0f;
                    Owner.m_AssetPathsNotReadyOrFailure.Add(Path);
                    var resourceCache = Owner.EnsureResourceCache(ResourcePath);
                    resourceCache.IncreaseRetainCount();

                    if (DependencyAssetPaths.Count <= 0)
                    {
                        CoreLog.DebugFormat("[AssetCache Init] {0} no dep. observe resource.", Path);
                        Status = AssetCacheStatus.WaitingForResource;

                        resourceCache.AddObserver(this);
                        return;
                    }

                    Status = AssetCacheStatus.WaitingForDeps;

                    foreach (var depAssetPath in DependencyAssetPaths)
                    {
                        var depAssetCache = Owner.EnsureAssetCache(depAssetPath);
                        depAssetCache.IncreaseRetainCount();
                        depAssetCache.AddObserver(this);
                    }

                    DFSAddResourceRetainCounts(resourceCache);
                }

                private void DFSAddResourceRetainCountsInternal(ResourceCache resourceCache, IDictionary<string, ResourceBasicInfo> resourceBasicInfos)
                {
                    s_DFSVisitedFlag.Add(resourceCache.Path);
                    var resourceBasicInfo = resourceBasicInfos[resourceCache.Path];
                    foreach (var dependencyResourcePath in resourceBasicInfo.DependencyResourcePaths)
                    {
                        if (s_DFSVisitedFlag.Contains(dependencyResourcePath))
                        {
                            continue;
                        }

                        var dependencyResourceCache = Owner.EnsureResourceCache(dependencyResourcePath);
                        dependencyResourceCache.IncreaseRetainCount();
                        m_DependencyResourcePaths.Add(dependencyResourcePath);
                        DFSAddResourceRetainCountsInternal(dependencyResourceCache, resourceBasicInfos);
                    }
                }

                private void DFSAddResourceRetainCounts(ResourceCache resourceCache)
                {
                    s_DFSVisitedFlag.Clear();
                    var resourceBasicInfos = Owner.ReadWriteIndex.ResourceBasicInfos;
                    DFSAddResourceRetainCountsInternal(resourceCache, resourceBasicInfos);
                }

                protected override void Update(TimeStruct timeStruct)
                {
                    switch (Status)
                    {
                        case AssetCacheStatus.WaitingForSlot:
                            if (IsScene)
                            {
                                SucceedAndNotify();
                            }
                            else if (Owner.m_RunningAssetLoadingTasks.Count < Owner.m_RunningAssetLoadingTasks.Capacity)
                            {
                                m_LoadingTask = Owner.RunAssetLoadingTask(Path, Owner.EnsureResourceCache(ResourcePath).ResourceObject);
                                CoreLog.DebugFormat("[AssetCache Update] {0} start loading.", Path);
                                Status = AssetCacheStatus.Loading;
                            }

                            break;

                        case AssetCacheStatus.Loading:
                            if (!string.IsNullOrEmpty(m_LoadingTask.ErrorMessage))
                            {
                                ErrorMessage = m_LoadingTask.ErrorMessage;
                                CoreLog.DebugFormat("[AssetCache Update] {0} loading fail.", Path);
                                FailAndNotify();
                            }
                            else if (m_LoadingTask.IsDone)
                            {
                                AssetObject = m_LoadingTask.AssetObject;
                                CoreLog.DebugFormat("[AssetCache Update] {0} loading success.", Path);
                                SucceedAndNotify();
                            }
                            else
                            {
                                if (LoadingProgress != m_LastLoadingProgress)
                                {
                                    m_LastLoadingProgress = LoadingProgress;
                                    ProgressAndNotify();
                                }
                            }

                            break;
                    }
                }

                internal override void Reset()
                {
                    CoreLog.DebugFormat("[AssetCache Reset] {0}", Path);
                    m_CopiedAssetObservers.Clear();
                    m_AssetObservers.Clear();
                    m_CopiedAssetAccessors.Clear();
                    m_AssetAccessors.Clear();
                    StopTicking();
                    StopAndResetLoadingTask();
                    AssetObject = null;

                    foreach (var depAssetPath in DependencyAssetPaths)
                    {
                        var depAssetCache = Owner.EnsureAssetCache(depAssetPath);
                        depAssetCache.RemoveObserver(this);
                        depAssetCache.ReduceRetainCount();
                    }

                    var resourceCache = Owner.EnsureResourceCache(ResourcePath);
                    resourceCache.RemoveObserver(this);
                    resourceCache.ReduceRetainCount();

                    foreach (var dependencyResourcePath in m_DependencyResourcePaths)
                    {
                        var dependencyResourceCache = Owner.m_ResourceCaches[dependencyResourcePath];
#if DEBUG
                        if (dependencyResourceCache.Status == ResourceCacheStatus.None)
                        {
                            throw new InvalidOperationException($"Resource cache of path [{dependencyResourcePath}] is invalid.");
                        }
#endif
                        dependencyResourceCache.ReduceRetainCount();
                    }

                    m_DependencyResourcePaths.Clear();

                    DependencyAssetPaths = null;
                    Status = AssetCacheStatus.None;
                    Owner.m_AssetPathsNotReadyOrFailure.Remove(Path);
                    m_DependencyAssetReadyCount = 0;
                    ResourcePath = null;
                    IsScene = false;
                    m_LastLoadingProgress = 0;
                    base.Reset();
                }

                internal void AddAccessor(AssetAccessor assetAccessor)
                {
                    if (Status == AssetCacheStatus.Ready)
                    {
                        CallLoadAssetSuccess(assetAccessor);
                    }
                    else if (Status == AssetCacheStatus.Failure)
                    {
                        CallLoadAssetFailureOrThrow(assetAccessor, ErrorMessage);
                    }
                    else
                    {
                        m_AssetAccessors.Add(assetAccessor);
                    }
                }

                internal bool RemoveAccessor(AssetAccessor assetAccessor)
                {
                    return m_AssetAccessors.Remove(assetAccessor);
                }

                internal void AddObserver(AssetCache assetObserver)
                {
                    if (Status == AssetCacheStatus.Ready)
                    {
                        assetObserver.OnLoadDependencyAssetSuccess(Path, AssetObject);
                    }
                    else if (Status == AssetCacheStatus.Failure)
                    {
                        assetObserver.OnLoadDependencyAssetFailure(Path, ErrorMessage);
                    }
                    else
                    {
                        m_AssetObservers.Add(assetObserver);
                    }
                }

                internal bool RemoveObserver(AssetCache assetObserver)
                {
                    return m_AssetObservers.Remove(assetObserver);
                }

                private void OnLoadDependencyAssetSuccess(string assetPath, object asset)
                {
                    if (Status == AssetCacheStatus.Failure)
                    {
                        return;
                    }

                    if (++m_DependencyAssetReadyCount < DependencyAssetPaths.Count)
                    {
                        return;
                    }

                    Status = AssetCacheStatus.WaitingForResource;
                    Owner.EnsureResourceCache(ResourcePath).AddObserver(this);
                }

                private void OnLoadDependencyAssetFailure(string assetPath, string errorMessage)
                {
                    if (Status == AssetCacheStatus.Failure)
                    {
                        return;
                    }

                    ErrorMessage = Utility.Text.Format("Load dependency asset failure. Resource path: '{0}'. Inner error: [{1}].",
                        assetPath, errorMessage);
                    FailAndNotify();
                }

                internal void OnLoadResourceSuccess(string resourcePath, object resource)
                {
                    if (Status == AssetCacheStatus.Failure)
                    {
                        return;
                    }

                    Status = AssetCacheStatus.WaitingForSlot;
                    StartTicking();
                }

                internal void OnLoadResourceFailure(string resourcePath, string errorMessage)
                {
                    if (Status == AssetCacheStatus.Failure)
                    {
                        return;
                    }

                    ErrorMessage = Utility.Text.Format("Load resource failure. Resource path: '{0}'. Inner error: [{1}].", resourcePath,
                        errorMessage);
                    FailAndNotify();
                }

                private void FailAndNotify()
                {
                    Status = AssetCacheStatus.Failure;
                    Owner.m_AssetPathsNotReadyOrFailure.Remove(Path);
                    StopTicking();
                    StopAndResetLoadingTask();

                    m_CopiedAssetObservers.Clear();
                    m_CopiedAssetObservers.AddRange(m_AssetObservers);
                    foreach (var assetObserver in m_CopiedAssetObservers)
                    {
                        assetObserver.OnLoadDependencyAssetFailure(Path, ErrorMessage);
                    }

                    m_CopiedAssetObservers.Clear();

                    m_AssetObservers.Clear();

                    m_CopiedAssetAccessors.Clear();
                    m_CopiedAssetAccessors.AddRange(m_AssetAccessors);
                    foreach (var assetAccessor in m_CopiedAssetAccessors)
                    {
                        CallLoadAssetFailureOrThrow(assetAccessor, ErrorMessage);
                    }

                    m_CopiedAssetAccessors.Clear();

                    m_AssetAccessors.Clear();

                    if (RetainCount <= 0)
                    {
                        MarkAsUnretained();
                    }
                }

                private void SucceedAndNotify()
                {
                    Status = AssetCacheStatus.Ready;
                    Owner.m_AssetPathsNotReadyOrFailure.Remove(Path);
                    StopAndResetLoadingTask();

                    m_CopiedAssetObservers.Clear();
                    m_CopiedAssetObservers.AddRange(m_AssetObservers);
                    foreach (var assetObserver in m_AssetObservers)
                    {
                        assetObserver.OnLoadDependencyAssetSuccess(Path, AssetObject);
                    }

                    m_CopiedAssetObservers.Clear();

                    m_AssetObservers.Clear();

                    m_CopiedAssetAccessors.Clear();
                    m_CopiedAssetAccessors.AddRange(m_AssetAccessors);
                    foreach (var assetAccessor in m_CopiedAssetAccessors)
                    {
                        CallLoadAssetSuccess(assetAccessor);
                    }

                    m_CopiedAssetAccessors.Clear();

                    m_AssetAccessors.Clear();

                    if (RetainCount <= 0)
                    {
                        MarkAsUnretained();
                    }
                }

                private void ProgressAndNotify()
                {
                    m_CopiedAssetAccessors.Clear();
                    m_CopiedAssetAccessors.AddRange(m_AssetAccessors);
                    foreach (var assetAccessor in m_CopiedAssetAccessors)
                    {
                        CallLoadAssetProgress(assetAccessor, LoadingProgress);
                    }

                    m_CopiedAssetAccessors.Clear();
                }

                private void StopAndResetLoadingTask()
                {
                    if (m_LoadingTask != null)
                    {
                        Owner.StopAndResetAssetLoadingTask(m_LoadingTask);
                        m_LoadingTask = null;
                    }
                }

                protected override void MarkAsUnretained()
                {
                    Owner.m_UnretainedAssetCaches.Add(this);
                }

                protected override void UnmarkAsUnretained()
                {
                    Owner.m_UnretainedAssetCaches.Remove(this);
                }

                internal override void IncreaseRetainCount()
                {
                    base.IncreaseRetainCount();
                    if (RetainCount > 0)
                    {
                        UnmarkAsUnretained();
                    }
                }

                internal override void ReduceRetainCount()
                {
                    base.ReduceRetainCount();
                    if (RetainCount <= 0 && (Status == AssetCacheStatus.Failure || Status == AssetCacheStatus.Ready))
                    {
                        MarkAsUnretained();
                    }
                }

                private static void CallLoadAssetFailureOrThrow(AssetAccessor assetAccessor, string errorMessage)
                {
                    var callbackSet = assetAccessor.CallbackSet;
                    if (callbackSet.OnFailure != null)
                    {
                        try
                        {
                            callbackSet.OnFailure(assetAccessor, errorMessage, assetAccessor.Context);
                        }
                        finally
                        {
                            assetAccessor.ResetCallbacks();
                        }
                    }
                    else
                    {
                        assetAccessor.ResetCallbacks();
                        throw new InvalidOperationException(errorMessage);
                    }
                }

                private static void CallLoadAssetSuccess(AssetAccessor assetAccessor)
                {
                    try
                    {
                        assetAccessor.CallbackSet.OnSuccess?.Invoke(assetAccessor, assetAccessor.Context);
                    }
                    finally
                    {
                        assetAccessor.ResetCallbacks();
                    }
                }

                private static void CallLoadAssetProgress(AssetAccessor assetAccessor, float progress)
                {
                    assetAccessor.CallbackSet.OnProgress?.Invoke(assetAccessor, progress, assetAccessor.Context);
                }
            }
        }
    }
}
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Asset
{
    public partial class AssetService
    {
        internal partial class Loader
        {
            private class ResourceCache : BaseCache
            {
                private ResourceLoadingTask m_LoadingTask = null;
                private readonly List<AssetCache> m_ResourceObservers = new List<AssetCache>();
                private readonly List<AssetCache> m_CopiedResourceObservers = new List<AssetCache>();

                internal object ResourceObject = null;

                internal bool ShouldLoadFromReadWritePath = false;

                public ResourceCacheStatus Status { get; private set; }

                public override float LoadingProgress => m_LoadingTask?.Progress ?? 0f;

                public ResourceCache() : base()
                {
                }

                internal override void Init()
                {
                    CoreLog.DebugFormat("[ResourceCache Reuse] {0}", Path);
                    Owner.m_ResourcePathsNotReadyOrFailure.Add(Path);
                    Status = ResourceCacheStatus.WaitingForSlot;
                    StartTicking();
                }

                internal override void Reset()
                {
                    CoreLog.DebugFormat("[ResourceCache Reset] {0}", Path);

                    m_CopiedResourceObservers.Clear();
                    m_ResourceObservers.Clear();

                    StopTicking();
                    StopAndResetLoadingTask();

                    if (ResourceObject != null)
                    {
                        Owner.ResourceDestroyer.Destroy(ResourceObject);
                    }

                    Path = null;
                    ResourceObject = null;
                    ShouldLoadFromReadWritePath = false;
                    Status = ResourceCacheStatus.None;
                    Owner.m_ResourcePathsNotReadyOrFailure.Remove(Path);
                    base.Reset();
                }

                protected override void Update(TimeStruct timeStruct)
                {
                    switch (Status)
                    {
                        case ResourceCacheStatus.WaitingForSlot:
                            if (Owner.m_RunningResourceLoadingTasks.Count < Owner.m_RunningResourceLoadingTasks.Capacity)
                            {
                                m_LoadingTask = Owner.RunResourceLoadingTask(Path,
                                    ShouldLoadFromReadWritePath ? Owner.ReadWritePath : Owner.InstallerPath);
                                CoreLog.DebugFormat("[ResourceCache Update] {0} start loading", Path);
                                Status = ResourceCacheStatus.Loading;
                            }

                            break;

                        case ResourceCacheStatus.Loading:
                            if (!string.IsNullOrEmpty(m_LoadingTask.ErrorMessage))
                            {
                                ErrorMessage = m_LoadingTask.ErrorMessage;

                                CoreLog.DebugFormat("[ResourceCache Update] {0} loading fail", Path);
                                FailAndNotify();
                            }
                            else if (m_LoadingTask.IsDone)
                            {
                                ResourceObject = m_LoadingTask.ResourceObject;

                                CoreLog.DebugFormat("[ResourceCache Update] {0} loading success", Path);
                                SucceedAndNotify();
                            }

                            break;

                        default:
                            break;
                    }
                }

                public void AddObserver(AssetCache resourceObserver)
                {
                    if (Status == ResourceCacheStatus.Ready)
                    {
                        CoreLog.DebugFormat("[ResourceCache AddObserver] Path={0}, AssetPath={1} direct success", Path,
                            resourceObserver.Path);
                        resourceObserver.OnLoadResourceSuccess(Path, ResourceObject);
                    }
                    else if (Status == ResourceCacheStatus.Failure)
                    {
                        CoreLog.DebugFormat("[ResourceCache AddObserver] Path={0}, AssetPath={1} direct fail", Path, resourceObserver.Path);
                        resourceObserver.OnLoadResourceFailure(Path, ErrorMessage);
                    }
                    else
                    {
                        m_ResourceObservers.Add(resourceObserver);
                    }
                }

                public bool RemoveObserver(AssetCache resourceObserver)
                {
                    return m_ResourceObservers.Remove(resourceObserver);
                }

                protected override void MarkAsUnretained()
                {
                    Owner.m_UnretainedResourceCaches.Add(this);
                }

                protected override void UnmarkAsUnretained()
                {
                    Owner.m_UnretainedResourceCaches.Remove(this);
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
                    if (RetainCount <= 0 && (Status == ResourceCacheStatus.Failure || Status == ResourceCacheStatus.Ready))
                    {
                        MarkAsUnretained();
                    }
                }

                private void StopAndResetLoadingTask()
                {
                    if (m_LoadingTask == null) return;
                    Owner.StopAndResetResourceLoadingTask(m_LoadingTask);
                    m_LoadingTask = null;
                }

                private void FailAndNotify()
                {
                    Status = ResourceCacheStatus.Failure;
                    Owner.m_ResourcePathsNotReadyOrFailure.Remove(Path);

                    StopTicking();
                    StopAndResetLoadingTask();

                    m_CopiedResourceObservers.Clear();
                    m_CopiedResourceObservers.AddRange(m_ResourceObservers);
                    foreach (var resourceObserver in m_CopiedResourceObservers)
                    {
                        resourceObserver.OnLoadResourceFailure(Path, ErrorMessage);
                    }

                    m_CopiedResourceObservers.Clear();

                    m_ResourceObservers.Clear();

                    if (RetainCount <= 0)
                    {
                        MarkAsUnretained();
                    }
                }

                private void SucceedAndNotify()
                {
                    Status = ResourceCacheStatus.Ready;
                    Owner.m_ResourcePathsNotReadyOrFailure.Remove(Path);
                    StopAndResetLoadingTask();

                    m_CopiedResourceObservers.Clear();
                    m_CopiedResourceObservers.AddRange(m_ResourceObservers);
                    foreach (var resourceObserver in m_CopiedResourceObservers)
                    {
                        resourceObserver.OnLoadResourceSuccess(Path, ResourceObject);
                    }

                    m_CopiedResourceObservers.Clear();

                    m_ResourceObservers.Clear();

                    if (RetainCount <= 0)
                    {
                        MarkAsUnretained();
                    }
                }
            }
        }
    }
}
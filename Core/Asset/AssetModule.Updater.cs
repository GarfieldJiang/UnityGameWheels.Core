using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace COL.UnityGameWheels.Core.Asset
{
    public partial class AssetModule
    {
        private partial class Updater : IResourceUpdater
        {
            private readonly AssetModule m_Owner;
            private ResourceGroupUpdateCallbackSet m_CallbackSet;
            private object m_Context;
            private readonly HashSet<int> m_DownloadTaskIds = new HashSet<int>();
            private int[] m_AvailableResourceGroupIds;
            private int m_ResourceGroupBeingUpdated = -1;
            private ResourceGroupUpdateSummary m_ResourceSummaryBeingUpdated = null;
            private Dictionary<int, ResourceGroupUpdateSummary> ResourceSummaries => m_Owner.ResourceGroupUpdateSummaries;

            private List<Uri> RootUrls => m_Owner.m_UpdateServerRootUrls;

            public bool IsReady => m_Owner.m_UpdateChecker.Status == UpdateCheckerStatus.Success;

            public AssetIndexForReadWrite ReadWriteIndex => m_Owner.m_ReadWriteIndex;

            private readonly OnDownloadFailure m_OnDownloadFailure;
            private readonly OnDownloadSuccess m_OnDownloadSuccess;
            private readonly OnDownloadProgress m_OnDownloadProgress;

            private long m_UpdatedBytesBeforeSavingReadWriteIndex = 0;

            public Updater(AssetModule owner)
            {
                m_Owner = owner;
                m_OnDownloadFailure = OnDownloadFailure;
                m_OnDownloadSuccess = OnDownloadSuccess;
                m_OnDownloadProgress = OnDownloadProgress;
            }

            private void Fail(Exception e, string errorMessageFormat)
            {
                var downloadTaskIds = new HashSet<int>(m_DownloadTaskIds);
                foreach (var downloadTaskId in downloadTaskIds)
                {
                    m_Owner.DownloadModule.StopDownloading(downloadTaskId, true);
                }

                m_DownloadTaskIds.Clear();

                var errorMessage = e == null ? errorMessageFormat : Utility.Text.Format(errorMessageFormat, e.ToString());
                if (m_CallbackSet.OnAllFailure != null)
                {
                    m_CallbackSet.OnAllFailure(errorMessage, m_Context);
                }
                else
                {
                    if (e != null)
                    {
                        throw new Exception(string.Empty, e);
                    }
                    else
                    {
                        throw new Exception(errorMessage);
                    }
                }
            }

            private void SingleFail(string resourcePath, string errorMessage)
            {
                if (m_CallbackSet.OnSingleFailure != null)
                {
                    m_CallbackSet.OnSingleFailure(resourcePath, errorMessage, m_Context);
                }
                else
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }

            private void OnDownloadSuccess(int downloadTaskId, DownloadTaskInfo downloadTaskInfo)
            {
                m_DownloadTaskIds.Remove(downloadTaskId);

                var resourcePath = ((DownloadContext) downloadTaskInfo.Context).ResourcePath;
                m_ResourceSummaryBeingUpdated.ResourcePathToSizeMap.Remove(resourcePath);
                m_ResourceSummaryBeingUpdated.TotalSize -= downloadTaskInfo.Size;
                m_UpdatedBytesBeforeSavingReadWriteIndex += downloadTaskInfo.Size;

                m_Owner.m_ReadWriteIndex.ResourceInfos[resourcePath] = m_Owner.m_RemoteIndex.ResourceInfos[resourcePath];

                if (m_ResourceSummaryBeingUpdated.TotalSize <= 0 ||
                    m_UpdatedBytesBeforeSavingReadWriteIndex >= m_Owner.UpdateSizeBeforeSavingReadWriteIndex)
                {
                    m_UpdatedBytesBeforeSavingReadWriteIndex = 0;
                    try
                    {
                        m_Owner.SaveReadWriteIndex();
                    }
                    catch (Exception e)
                    {
                        string errorMessageFormat = "Cannot save read-write index. Inner exception is '{0}'.";
                        SingleFail(resourcePath, Utility.Text.Format("Cannot save read-write index. Inner exception is '{0}'.", e));
                        Fail(e, errorMessageFormat);
                        return;
                    }
                }

                m_CallbackSet.OnSingleSuccess?.Invoke(resourcePath, downloadTaskInfo.Size, m_Context);
                if (m_ResourceSummaryBeingUpdated.TotalSize <= 0L)
                {
                    ClearBeingUpdated();
                    m_CallbackSet.OnAllSuccess?.Invoke(m_Context);
                }
            }

            private void OnDownloadProgress(int downloadTaskId, DownloadTaskInfo downloadTaskInfo, long downloadedSize)
            {
                m_CallbackSet.OnSingleProgress?.Invoke(((DownloadContext) downloadTaskInfo.Context).ResourcePath, downloadedSize,
                    downloadTaskInfo.Size, m_Context);
            }

            private void OnDownloadFailure(int downloadTaskId, DownloadTaskInfo downloadTaskInfo, DownloadErrorCode errorCode,
                string errorMessage)
            {
                m_DownloadTaskIds.Remove(downloadTaskId);

                var downloadContext = (DownloadContext) downloadTaskInfo.Context;
                errorMessage = Utility.Text.Format(
                    "Download failed for '{0}' from '{1}'. Inner error code is '{2}'. Inner error message is '{3}'.",
                    downloadContext.ResourcePath, downloadTaskInfo.UrlStr, errorCode, errorMessage);

                if (downloadContext.RootUrlIndex >= RootUrls.Count - 1)
                {
                    SingleFail(downloadContext.ResourcePath, errorMessage);
                    ClearBeingUpdated();
                    Fail(null, errorMessage);
                    return;
                }

                downloadContext.RootUrlIndex++;
                var newDownloadTaskInfo = new DownloadTaskInfo(
                    Utility.Text.Format("{0}/{1}_{2}{3}", RootUrls[downloadContext.RootUrlIndex], downloadContext.ResourcePath,
                        downloadTaskInfo.Crc32.Value, Constant.ResourceFileExtension),
                    downloadTaskInfo.SavePath,
                    downloadTaskInfo.Size, downloadTaskInfo.Crc32, new DownloadCallbackSet
                    {
                        OnFailure = m_OnDownloadFailure,
                        OnSuccess = m_OnDownloadSuccess,
                        OnProgress = m_OnDownloadProgress,
                    }, downloadContext);
                m_DownloadTaskIds.Add(m_Owner.DownloadModule.StartDownloading(newDownloadTaskInfo));
            }

            private int[] AvailableResourceGroupIds
            {
                get
                {
                    if (m_AvailableResourceGroupIds == null)
                    {
                        m_AvailableResourceGroupIds = new int[ReadWriteIndex.ResourceGroupInfos.Count];
                        for (int i = 0; i < ReadWriteIndex.ResourceGroupInfos.Count; i++)
                        {
                            m_AvailableResourceGroupIds[i] = ReadWriteIndex.ResourceGroupInfos[i].GroupId;
                        }
                    }

                    return m_AvailableResourceGroupIds;
                }
            }

            public int[] GetAvailableResourceGroupIds()
            {
                if (!IsReady)
                {
                    throw new InvalidOperationException("Not ready.");
                }

                var ret = new int[AvailableResourceGroupIds.Length];
                for (int i = 0; i < AvailableResourceGroupIds.Length; i++)
                {
                    ret[i] = AvailableResourceGroupIds[i];
                }

                return ret;
            }

            public void GetAvailableResourceGroupIds(List<int> groupIds)
            {
                if (!IsReady)
                {
                    throw new InvalidOperationException("Not ready.");
                }

                if (groupIds == null)
                {
                    throw new ArgumentNullException("groupIds");
                }

                groupIds.Clear();
                for (int i = 0; i < AvailableResourceGroupIds.Length; i++)
                {
                    groupIds.Add(AvailableResourceGroupIds[i]);
                }
            }

            public ResourceGroupStatus GetResourceGroupStatus(int groupId)
            {
                if (!IsReady)
                {
                    throw new InvalidOperationException("Not ready.");
                }

                if (!AvailableResourceGroupIds.Contains(groupId))
                {
                    throw new ArgumentException(Utility.Text.Format("Resource group '{0}' is not available.", groupId));
                }

                if (m_ResourceGroupBeingUpdated == groupId)
                {
                    return ResourceGroupStatus.BeingUpdated;
                }

                return ResourceSummaries[groupId].TotalSize > 0 ? ResourceGroupStatus.OutOfDate : ResourceGroupStatus.UpToDate;
            }

            public ResourceGroupUpdateSummary GetResourceGroupUpdateSummary(int groupId)
            {
                if (!IsReady)
                {
                    throw new InvalidOperationException("Not ready.");
                }

                if (!AvailableResourceGroupIds.Contains(groupId))
                {
                    throw new ArgumentException(Utility.Text.Format("Resource group '{0}' is not available.", groupId));
                }

                ResourceGroupUpdateSummary resourceSummary;
                if (!ResourceSummaries.TryGetValue(groupId, out resourceSummary))
                {
                    throw new InvalidOperationException(Utility.Text.Format("Oops! Cannot find resource summary for group '{0}'.",
                        groupId));
                }

                return resourceSummary;
            }

            public void StartUpdatingResourceGroup(int groupId, ResourceGroupUpdateCallbackSet callbackSet, object context)
            {
                if (!IsReady)
                {
                    throw new InvalidOperationException("Not ready.");
                }

                if (m_ResourceGroupBeingUpdated >= 0)
                {
                    throw new InvalidOperationException(Utility.Text.Format("A resource group '{0}' is being update.",
                        m_ResourceGroupBeingUpdated));
                }

                if (!AvailableResourceGroupIds.Contains(groupId))
                {
                    throw new ArgumentException(Utility.Text.Format("Resource group '{0}' is not available.", groupId));
                }

                if (GetResourceGroupStatus(groupId) == ResourceGroupStatus.UpToDate)
                {
                    throw new InvalidOperationException(Utility.Text.Format("Resource group '{0}' is already up-to-date.", groupId));
                }

                if (AvailableResourceGroupIds.Contains(0) && groupId != 0 && GetResourceGroupStatus(0) != ResourceGroupStatus.UpToDate)
                {
                    throw new InvalidOperationException("You have to update resource group 0 first.");
                }

                var resourceSummary = ResourceSummaries[groupId];

                m_ResourceGroupBeingUpdated = groupId;
                m_ResourceSummaryBeingUpdated = resourceSummary;
                m_CallbackSet = callbackSet;
                m_Context = context;

                foreach (var resourceToUpdate in resourceSummary)
                {
                    var downloadContext = new DownloadContext {RootUrlIndex = 0, ResourcePath = resourceToUpdate.Key};
                    var resourceInfo = m_Owner.m_RemoteIndex.ResourceInfos[resourceToUpdate.Key];
                    var downloadTaskInfo = new DownloadTaskInfo(
                        Utility.Text.Format("{0}/{1}_{2}{3}", RootUrls[0], resourceToUpdate.Key, resourceInfo.Crc32,
                            Constant.ResourceFileExtension),
                        Path.Combine(m_Owner.ReadWritePath, resourceToUpdate.Key + Constant.ResourceFileExtension),
                        resourceInfo.Size, resourceInfo.Crc32, new DownloadCallbackSet
                        {
                            OnFailure = m_OnDownloadFailure,
                            OnSuccess = m_OnDownloadSuccess,
                            OnProgress = m_OnDownloadProgress,
                        }, downloadContext);
                    m_DownloadTaskIds.Add(m_Owner.DownloadModule.StartDownloading(downloadTaskInfo));
                }
            }

            public bool StopUpdatingResourceGroup(int groupId)
            {
                if (!IsReady)
                {
                    throw new InvalidOperationException("Not ready.");
                }

                if (!AvailableResourceGroupIds.Contains(groupId))
                {
                    throw new ArgumentException(Utility.Text.Format("Resource group '{0}' is not available.", groupId));
                }

                if (groupId != m_ResourceGroupBeingUpdated)
                {
                    return false;
                }

                foreach (var downloadTaskId in m_DownloadTaskIds)
                {
                    m_Owner.DownloadModule.StopDownloading(downloadTaskId, true);
                }

                m_DownloadTaskIds.Clear();
                ClearBeingUpdated();
                return true;
            }

            private void ClearBeingUpdated()
            {
                m_UpdatedBytesBeforeSavingReadWriteIndex = 0L;
                m_ResourceGroupBeingUpdated = -1;
                m_ResourceSummaryBeingUpdated = null;
            }
        }
    }
}
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
            private int[] m_AvailableResourceGroupIds;
            private readonly Dictionary<int, ResourceGroupBeingUpdated> m_ResourceGroupsBeingUpdated = new Dictionary<int, ResourceGroupBeingUpdated>();

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

            private void Fail(ResourceGroupBeingUpdated resourceGroup, Exception e, string errorMessageFormat)
            {
                foreach (var downloadTaskId in resourceGroup.DownloadTaskIds)
                {
                    m_Owner.DownloadModule.StopDownloading(downloadTaskId, true);
                }

                resourceGroup.DownloadTaskIds.Clear();

                var errorMessage = e == null ? errorMessageFormat : Utility.Text.Format(errorMessageFormat, e.ToString());
                if (resourceGroup.CallbackSet.OnAllFailure != null)
                {
                    resourceGroup.CallbackSet.OnAllFailure(errorMessage, resourceGroup.CallbackContext);
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

            private void SingleFail(ResourceGroupBeingUpdated resourceGroup, string resourcePath, string errorMessage)
            {
                if (resourceGroup.CallbackSet.OnSingleFailure != null)
                {
                    resourceGroup.CallbackSet.OnSingleFailure(resourcePath, errorMessage, resourceGroup.CallbackContext);
                }
                else
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }

            private void OnDownloadSuccess(int downloadTaskId, DownloadTaskInfo downloadTaskInfo)
            {
                var downloadContext = (DownloadContext)downloadTaskInfo.Context;
                var resourcePath = downloadContext.ResourcePath;
                var resourceGroupId = downloadContext.ResourceGroupId;
                var resourceGroup = m_ResourceGroupsBeingUpdated[resourceGroupId];
                resourceGroup.DownloadTaskIds.Remove(downloadTaskId);
                var resourceSummary = resourceGroup.Summary;
                resourceSummary.ResourcePathToSizeMap.Remove(resourcePath);
                resourceSummary.RemainingSize -= downloadTaskInfo.Size;
                m_UpdatedBytesBeforeSavingReadWriteIndex += downloadTaskInfo.Size;

                m_Owner.m_ReadWriteIndex.ResourceInfos[resourcePath] = m_Owner.m_RemoteIndex.ResourceInfos[resourcePath];

                if (resourceSummary.RemainingSize <= 0 ||
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
                        SingleFail(resourceGroup, resourcePath, Utility.Text.Format(errorMessageFormat, e));
                        ClearBeingUpdated(resourceGroupId);
                        Fail(resourceGroup, e, errorMessageFormat);
                        return;
                    }
                }

                resourceGroup.CallbackSet.OnSingleSuccess?.Invoke(resourcePath, downloadTaskInfo.Size, resourceGroup.CallbackContext);
                if (resourceSummary.RemainingSize <= 0L)
                {
                    ClearBeingUpdated(resourceGroupId);
                    resourceGroup.CallbackSet.OnAllSuccess?.Invoke(resourceGroup.CallbackContext);
                }
            }

            private void OnDownloadProgress(int downloadTaskId, DownloadTaskInfo downloadTaskInfo, long downloadedSize)
            {
                var downloadContext = (DownloadContext)downloadTaskInfo.Context;
                var resourceGroup = m_ResourceGroupsBeingUpdated[downloadContext.ResourceGroupId];
                resourceGroup.CallbackSet.OnSingleProgress?.Invoke(((DownloadContext)downloadTaskInfo.Context).ResourcePath, downloadedSize,
                    downloadTaskInfo.Size, resourceGroup.CallbackContext);
            }

            private void OnDownloadFailure(int downloadTaskId, DownloadTaskInfo downloadTaskInfo, DownloadErrorCode errorCode,
                string errorMessage)
            {
                var downloadContext = (DownloadContext)downloadTaskInfo.Context;
                var resourceGroup = m_ResourceGroupsBeingUpdated[downloadContext.ResourceGroupId];
                resourceGroup.DownloadTaskIds.Remove(downloadTaskId);
                errorMessage = Utility.Text.Format(
                    "Download failed for '{0}' from '{1}'. Inner error code is '{2}'. Inner error message is '{3}'.",
                    downloadContext.ResourcePath, downloadTaskInfo.UrlStr, errorCode, errorMessage);

                if (downloadContext.RootUrlIndex >= RootUrls.Count - 1)
                {
                    SingleFail(resourceGroup, downloadContext.ResourcePath, errorMessage);
                    ClearBeingUpdated(resourceGroup.GroupId);
                    Fail(resourceGroup, null, errorMessage);
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
                resourceGroup.DownloadTaskIds.Add(m_Owner.DownloadModule.StartDownloading(newDownloadTaskInfo));
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

                if (m_ResourceGroupsBeingUpdated.ContainsKey(groupId))
                {
                    return ResourceGroupStatus.BeingUpdated;
                }

                return ResourceSummaries[groupId].RemainingSize > 0 ? ResourceGroupStatus.OutOfDate : ResourceGroupStatus.UpToDate;
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

                if (!ResourceSummaries.TryGetValue(groupId, out var resourceSummary))
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

                if (m_ResourceGroupsBeingUpdated.ContainsKey(groupId))
                {
                    throw new InvalidOperationException($"Resource group '{groupId}' is being updated.");
                }

                var resourceSummary = ResourceSummaries[groupId];
                var resourceGroup = new ResourceGroupBeingUpdated
                {
                    GroupId = groupId,
                    Summary = resourceSummary,
                    CallbackSet = callbackSet,
                    CallbackContext = context,
                };
                m_ResourceGroupsBeingUpdated.Add(groupId, resourceGroup);

                foreach (var resourceToUpdate in resourceSummary)
                {
                    var downloadContext = new DownloadContext
                    {
                        RootUrlIndex = 0,
                        ResourcePath = resourceToUpdate.Key,
                        ResourceGroupId = groupId,
                    };
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
                    resourceGroup.DownloadTaskIds.Add(m_Owner.DownloadModule.StartDownloading(downloadTaskInfo));
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

                if (m_ResourceGroupsBeingUpdated.TryGetValue(groupId, out var resourceGroup))
                {
                    return false;
                }

                foreach (var downloadTaskId in resourceGroup.DownloadTaskIds)
                {
                    m_Owner.DownloadModule.StopDownloading(downloadTaskId, true);
                }

                resourceGroup.DownloadTaskIds.Clear();
                ClearBeingUpdated(groupId);
                return true;
            }

            private void ClearBeingUpdated(int resourceGroupId)
            {
                m_UpdatedBytesBeforeSavingReadWriteIndex = 0L;
                m_ResourceGroupsBeingUpdated.Remove(resourceGroupId);
            }

            private class ResourceGroupBeingUpdated
            {
                public int GroupId;
                public ResourceGroupUpdateSummary Summary;
                public ResourceGroupUpdateCallbackSet CallbackSet;
                public object CallbackContext;
                public readonly HashSet<int> DownloadTaskIds = new HashSet<int>();
            }
        }
    }
}
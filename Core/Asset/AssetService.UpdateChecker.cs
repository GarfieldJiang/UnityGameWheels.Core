﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace COL.UnityGameWheels.Core.Asset
{
    public partial class AssetService
    {
        private class UpdateChecker
        {
            private readonly AssetService m_Owner;
            private AssetIndexRemoteFileInfo m_RemoteIndexFileInfo;
            private UpdateCheckCallbackSet m_CallbackSet;
            private object m_Context;
            private int m_RootUrlIndex = 0;
            private int m_DownloadRetryTimes = 0;
            private DownloadTaskInfo m_DownloadTaskInfo;
            private readonly OnDownloadSuccess m_OnDownloadSuccess;
            private readonly OnDownloadFailure m_OnDownloadFailure;

            private static int s_IsCalculatingRemoteIndexCheckSum;
            private static int s_IsUnzippingRemoteIndex;
            private Task<bool> m_CheckNeedDownloadRemoteIndexSubtask;
            private Task m_UnzipRemoteIndexSubtask;
            private UpdateCheckerRunStatus m_RunStatus = UpdateCheckerRunStatus.None;

            public UpdateCheckerStatus Status { get; private set; }

            private bool m_RootUrlsModified = false;
            private List<Uri> RootUrls => m_Owner.m_UpdateServerRootUrls;

            private AssetIndexForInstaller InstallerIndex => m_Owner.m_InstallerIndex;

            private AssetIndexForReadWrite ReadWriteIndex => m_Owner.m_ReadWriteIndex;

            private AssetIndexForRemote RemoteIndex => m_Owner.m_RemoteIndex;

            private Dictionary<int, ResourceGroupUpdateSummary> ResourceSummaries =>
                m_Owner.ResourceGroupUpdateSummaries;

            private readonly HashSet<string> m_ResourcesToDelete = new HashSet<string>();

            public UpdateChecker(AssetService owner)
            {
                m_Owner = owner;
                ResetStatus();
                m_OnDownloadFailure = OnDownloadFailure;
                m_OnDownloadSuccess = OnDownloadSuccess;
            }

            public void Run(AssetIndexRemoteFileInfo remoteIndexFileInfo, UpdateCheckCallbackSet callbackSet,
                object context)
            {
                if (Status != UpdateCheckerStatus.None)
                {
                    throw new InvalidOperationException("Update checking already run.");
                }

                Status = UpdateCheckerStatus.Running;

                m_CallbackSet = callbackSet;
                m_Context = context;

                if (!m_Owner.UpdateIsEnabled)
                {
                    UseInstallerResourcesOnly();
                    return;
                }

                m_RemoteIndexFileInfo = remoteIndexFileInfo ??
                                        throw new InvalidOperationException("Remote index file info is invalid.");

                if (RootUrls.Count <= 0)
                {
                    ResetStatus();
                    throw new InvalidOperationException("Root URL for any update server hasn't been set.");
                }

                // TODO: Make a better impl of root urls
                if (!m_RootUrlsModified)
                {
                    m_RootUrlsModified = true;
                    for (int i = 0; i < RootUrls.Count; i++)
                    {
                        RootUrls[i] = new Uri(RootUrls[i], Utility.Text.Format(m_Owner.UpdateRelativePathFormat,
                            m_Owner.RunningPlatform,
                            $"{m_Owner.BundleVersion}.{m_RemoteIndexFileInfo.InternalAssetVersion}"));
                    }
                }

                m_RunStatus = UpdateCheckerRunStatus.Waiting;
            }

            private bool CheckNeedDownloadRemoteIndex(AssetIndexRemoteFileInfo remoteIndexFileInfo)
            {
                try
                {
                    bool needDownloadRemoteIndex = true;
                    var cachedRemoteIndexFile = new FileInfo(m_Owner.CachedRemoteIndexPath);
                    if (cachedRemoteIndexFile.Exists && cachedRemoteIndexFile.Length == remoteIndexFileInfo.FileSize)
                    {
                        using (var stream = cachedRemoteIndexFile.OpenRead())
                        {
                            if (Algorithm.Crc32.Sum(stream) == remoteIndexFileInfo.Crc32)
                            {
                                needDownloadRemoteIndex = false;
                            }
                        }
                    }

                    return needDownloadRemoteIndex;
                }
                finally
                {
                    Interlocked.Exchange(ref s_IsCalculatingRemoteIndexCheckSum, 0);
                }
            }

            private void UseInstallerResourcesOnly()
            {
                if (!m_Owner.TryCleanUpReadWritePathOrFail(Fail))
                {
                    return;
                }

                ReadWriteIndex.AssetInfos.Clear();
                ReadWriteIndex.ResourceInfos.Clear();
                ReadWriteIndex.ResourceGroupInfos.Clear();
                ReadWriteIndex.ResourceBasicInfos.Clear();

                foreach (var resourceGroupInfo in InstallerIndex.ResourceGroupInfos)
                {
                    ReadWriteIndex.ResourceGroupInfos.Add(resourceGroupInfo);
                    ResourceSummaries.Add(resourceGroupInfo.GroupId, new ResourceGroupUpdateSummary());
                }

                foreach (var kv in InstallerIndex.AssetInfos)
                {
                    ReadWriteIndex.AssetInfos.Add(kv.Key, kv.Value);
                }

                foreach (var kv in InstallerIndex.ResourceBasicInfos)
                {
                    ReadWriteIndex.ResourceBasicInfos.Add(kv.Key, kv.Value);
                }

                if (!m_Owner.TrySaveReadWriteIndexOrFail(Fail))
                {
                    return;
                }

                Succeed();
            }

            private void OnDownloadSuccess(int downloadTaskId, DownloadTaskInfo taskInfo)
            {
                StartUnzippingRemoteIndex();
            }

            private void ResetStatus()
            {
                Status = UpdateCheckerStatus.None;
                m_RunStatus = UpdateCheckerRunStatus.None;
                m_RootUrlIndex = 0;
                m_DownloadRetryTimes = 0;
                ResourceSummaries.Clear();
                m_CheckNeedDownloadRemoteIndexSubtask = null;
                m_UnzipRemoteIndexSubtask = null;
            }


            private void OnDownloadFailure(int downloadTaskId, DownloadTaskInfo downloadTaskInfo,
                DownloadErrorCode errorCode,
                string errorMessage)
            {
                if (m_DownloadRetryTimes >= m_Owner.DownloadRetryCount)
                {
                    m_DownloadRetryTimes = -1;
                    m_RootUrlIndex++;
                    if (m_RootUrlIndex >= RootUrls.Count)
                    {
                        ResetStatus();
                        errorMessage = Utility.Text.Format(
                            "Cannot update remote index file. Error code is '{0}'. Error message is '{1}'.",
                            errorCode, errorMessage);
                        if (m_CallbackSet.OnFailure != null)
                        {
                            m_CallbackSet.OnFailure(errorMessage, m_Context);
                        }
                        else
                        {
                            throw new InvalidOperationException(errorMessage);
                        }

                        return;
                    }
                }

                m_DownloadRetryTimes++;

                if (m_DownloadRetryTimes == 0)
                {
                    m_DownloadTaskInfo = CreateDownloadTaskInfo();
                }

                m_Owner.m_DownloadService.StartDownloading(m_DownloadTaskInfo);
            }

            private void CheckUpdate()
            {
                if (!TryParseRemoteIndexOrFail())
                {
                    return;
                }

                CopyInfos();
                DoThreePartyComparison();

                if (!m_Owner.TrySaveReadWriteIndexOrFail(Fail))
                {
                    return;
                }

                if (!TryDeleteStaleResources())
                {
                    return;
                }

                if (!m_Owner.TrySaveReadWriteIndexOrFail(Fail))
                {
                    return;
                }

                Succeed();
            }

            private void CopyInfos()
            {
                m_Owner.m_ReadWriteIndex.AssetInfos.Clear();
                foreach (var kv in RemoteIndex.AssetInfos)
                {
                    m_Owner.m_ReadWriteIndex.AssetInfos.Add(kv.Key, kv.Value);
                }

                m_Owner.m_ReadWriteIndex.ResourceGroupInfos.Clear();
                foreach (var resourceGroupInfo in RemoteIndex.ResourceGroupInfos)
                {
                    m_Owner.m_ReadWriteIndex.ResourceGroupInfos.Add(resourceGroupInfo);
                }

                m_Owner.m_ReadWriteIndex.ResourceBasicInfos.Clear();
                foreach (var kv in RemoteIndex.ResourceBasicInfos)
                {
                    m_Owner.m_ReadWriteIndex.ResourceBasicInfos.Add(kv.Key, kv.Value);
                }
            }

            private void Succeed()
            {
                Status = UpdateCheckerStatus.Success;
                m_CallbackSet.OnSuccess?.Invoke(m_Context);
            }

            private void Fail(Exception e, string errorMessageFormat)
            {
                ResetStatus();
                var errorMessage = Utility.Text.Format(errorMessageFormat, e.ToString());
                if (m_CallbackSet.OnFailure != null)
                {
                    m_CallbackSet.OnFailure(errorMessage, m_Context);
                }
                else
                {
                    throw new Exception(string.Empty, e);
                }
            }

            private bool TryParseRemoteIndexOrFail()
            {
                try
                {
                    using (var fs = File.OpenRead(m_Owner.CachedRemoteIndexPath))
                    {
                        using (var br = new BinaryReader(fs))
                        {
                            new AssetIndexSerializerV2().FromBinary(br, RemoteIndex);
                        }
                    }
                }
                catch (Exception e)
                {
                    Fail(e, "Cannot parse cached remote index file. Inner exception is '{0}'.");
                    return false;
                }

                return true;
            }

            private void DoThreePartyComparison()
            {
                foreach (var readWrite in ReadWriteIndex.ResourceInfos.Values)
                {
                    if (!RemoteIndex.ResourceInfos.TryGetValue(readWrite.Path, out var remote))
                    {
                        m_ResourcesToDelete.Add(readWrite.Path);
                        continue;
                    }

                    if (!InstallerIndex.ResourceInfos.TryGetValue(readWrite.Path, out var installer))
                    {
                        continue;
                    }

                    if (remote.Hash == installer.Hash && remote.Size == installer.Size)
                    {
                        m_ResourcesToDelete.Add(readWrite.Path);
                    }
                }

                foreach (var resource in m_ResourcesToDelete)
                {
                    ReadWriteIndex.ResourceInfos.Remove(resource);
                }

                foreach (var resourceGroupInfo in ReadWriteIndex.ResourceGroupInfos)
                {
                    ResourceSummaries.Add(resourceGroupInfo.GroupId, new ResourceGroupUpdateSummary());
                }

                foreach (var remote in RemoteIndex.ResourceInfos.Values)
                {
                    var groupId = RemoteIndex.ResourceBasicInfos[remote.Path].GroupId;
                    var resourceSummary = ResourceSummaries[groupId];
                    resourceSummary.TotalSize += remote.Size;

                    if (InstallerIndex.ResourceInfos.TryGetValue(remote.Path, out var installer) &&
                        installer.Hash == remote.Hash &&
                        installer.Size == remote.Size)
                    {
                        continue;
                    }

                    if (ReadWriteIndex.ResourceInfos.TryGetValue(remote.Path, out var readWrite) &&
                        readWrite.Hash == remote.Hash &&
                        readWrite.Size == remote.Size)
                    {
                        continue;
                    }

                    resourceSummary.ResourcePathToSizeMap.Add(remote.Path, remote.Size);
                    resourceSummary.RemainingSize += remote.Size;
                }
            }

            private bool TryDeleteStaleResources()
            {
                try
                {
                    if (!Directory.Exists(m_Owner.ReadWritePath))
                    {
                        return true;
                    }

                    foreach (var resource in m_ResourcesToDelete)
                    {
                        var resourceAbsPath = Path.Combine(m_Owner.ReadWritePath,
                            resource + Constant.ResourceFileExtension);
                        if (File.Exists(resourceAbsPath))
                        {
                            File.Delete(resourceAbsPath);
                        }
                    }

                    Utility.IO.DeleteEmptyFolders(m_Owner.ReadWritePath);
                }
                catch (Exception e)
                {
                    Fail(e, "Cannot delete stale resources. Inner exception is '{0}'.");
                    return false;
                }

                return true;
            }


            internal void Update(TimeStruct timeStruct)
            {
                switch (m_RunStatus)
                {
                    case UpdateCheckerRunStatus.Waiting:
                        UpdateWaiting();
                        break;
                    case UpdateCheckerRunStatus.CheckingNeedDownloadRemoteIndex:
                        UpdateCheckingNeedDownloadRemoteIndex();
                        break;
                    case UpdateCheckerRunStatus.UnzippingRemoteIndex:
                        UpdateUnzippingRemoteIndex();
                        break;
                }
            }

            private void UpdateWaiting()
            {
                if (0 != Interlocked.CompareExchange(ref s_IsCalculatingRemoteIndexCheckSum, 0, 0) ||
                    0 != Interlocked.CompareExchange(ref s_IsUnzippingRemoteIndex, 0, 0)) return;
                m_RunStatus = UpdateCheckerRunStatus.CheckingNeedDownloadRemoteIndex;
                Interlocked.Exchange(ref s_IsCalculatingRemoteIndexCheckSum, 1);
                m_CheckNeedDownloadRemoteIndexSubtask = Task.Run(() => CheckNeedDownloadRemoteIndex(m_RemoteIndexFileInfo));
            }

            private void UpdateCheckingNeedDownloadRemoteIndex()
            {
                if (m_CheckNeedDownloadRemoteIndexSubtask.IsFaulted)
                {
                    var task = m_CheckNeedDownloadRemoteIndexSubtask;
                    m_CheckNeedDownloadRemoteIndexSubtask = null;
                    foreach (var e in m_CheckNeedDownloadRemoteIndexSubtask.Exception.InnerExceptions)
                    {
                        throw e;
                    }
                }
                else if (m_CheckNeedDownloadRemoteIndexSubtask.IsCompleted)
                {
                    var task = m_CheckNeedDownloadRemoteIndexSubtask;
                    m_CheckNeedDownloadRemoteIndexSubtask = null;
                    if (!task.Result)
                    {
                        StartUnzippingRemoteIndex();
                    }
                    else
                    {
                        StartDownloadingRemoteIndex();
                    }
                }
            }

            private void StartDownloadingRemoteIndex()
            {
                m_RunStatus = UpdateCheckerRunStatus.DownloadingRemoteIndex;
                m_DownloadRetryTimes = 0;
                m_DownloadTaskInfo = CreateDownloadTaskInfo();
                m_Owner.m_DownloadService.StartDownloading(m_DownloadTaskInfo);
            }

            private DownloadTaskInfo CreateDownloadTaskInfo()
            {
                return new DownloadTaskInfo($"{RootUrls[m_RootUrlIndex]}/index_{m_RemoteIndexFileInfo.Crc32.ToString()}.zip",
                    m_Owner.CachedZippedRemoteIndexPath, m_RemoteIndexFileInfo.FileSize, m_RemoteIndexFileInfo.Crc32,
                    new DownloadCallbackSet
                    {
                        OnSuccess = m_OnDownloadSuccess,
                        OnFailure = m_OnDownloadFailure,
                        OnProgress = null,
                    }, null);
            }

            private void StartUnzippingRemoteIndex()
            {
                m_RunStatus = UpdateCheckerRunStatus.UnzippingRemoteIndex;
                Interlocked.Exchange(ref s_IsUnzippingRemoteIndex, 1);
                m_UnzipRemoteIndexSubtask = Task.Run(() =>
                {
                    try
                    {
                        File.Delete(m_Owner.CachedRemoteIndexPath);
                        using (var zipFile = File.OpenRead(m_Owner.CachedZippedRemoteIndexPath))
                        {
                            using (var dstFile = File.OpenWrite(m_Owner.CachedRemoteIndexPath))
                            {
                                m_Owner.m_ZipImpl.Unzip(zipFile, dstFile);
                            }
                        }
                    }
                    finally
                    {
                        Interlocked.Exchange(ref s_IsUnzippingRemoteIndex, 0);
                    }
                });
            }

            private void UpdateUnzippingRemoteIndex()
            {
                if (m_UnzipRemoteIndexSubtask.IsFaulted)
                {
                    var task = m_UnzipRemoteIndexSubtask;
                    m_UnzipRemoteIndexSubtask = null;
                    foreach (var e in task.Exception.InnerExceptions)
                    {
                        throw e;
                    }
                }
                else if (m_UnzipRemoteIndexSubtask.IsCompleted)
                {
                    m_UnzipRemoteIndexSubtask = null;
                    m_RunStatus = UpdateCheckerRunStatus.None;
                    CheckUpdate();
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Default implementation of download module.
    /// </summary>
    public partial class DownloadService : TickableService, IDownloadService
    {
        private readonly IRefPoolService m_RefPoolService = null;

        public IDownloadServiceConfigReader ConfigReader { get; }

        private string m_TempFileExtension = null;

        private int m_ConcurrentDownloadCountLimit = 1;

        private int m_ChunkSizeToSave = 0;

        private float m_Timeout = 0f;

        private IRefPool<DownloadTask> m_DownloadTaskPool = null;

        /// <inheritdoc />
        public int ConcurrentDownloadCountLimit => m_ConcurrentDownloadCountLimit;

        /// <inheritdoc />
        public int ChunkSizeToSave => m_ChunkSizeToSave;

        /// <inheritdoc />
        public string TempFileExtension => m_TempFileExtension;

        /// <inheritdoc />
        public float Timeout => m_Timeout;

        /// <summary>
        /// Factory for a <see cref="IDownloadTaskImpl"/> instance.
        /// </summary>
        public ISimpleFactory<IDownloadTaskImpl> DownloadTaskImplFactory { get; }

        private readonly SortedDictionary<int, DownloadTaskInfoSlot> m_WaitingDownloadTaskInfoSlots = new SortedDictionary<int, DownloadTaskInfoSlot>();

        private IRefPool<DownloadTaskInfoSlot> m_DownloadTaskInfoSlotPool = null;

        private readonly Dictionary<int, IDownloadTask> m_OngoingDownloadTasks = new Dictionary<int, IDownloadTask>();

        private readonly Dictionary<int, long> m_CachedOngoingDownloadedSizes = new Dictionary<int, long>();

        private readonly List<int> m_DownloadTaskIdsToRemove = new List<int>();

        private int m_CurrentDownloadTaskId = 0;

        private readonly HashSet<int> m_QuietlyStopTaskIds = new HashSet<int>();

        public DownloadService(IDownloadServiceConfigReader configReader, IRefPoolService refPoolService, ITickService tickService,
            ISimpleFactory<IDownloadTaskImpl> downloadTaskImplFactory)
            : base(tickService)
        {
            ConfigReader = configReader;
            m_RefPoolService = refPoolService;
            DownloadTaskImplFactory = downloadTaskImplFactory;
            InitTempFileExtension();
            InitConcurrentDownloadCountLimit();
            m_ChunkSizeToSave = ConfigReader.ChunkSizeToSave;
            m_Timeout = ConfigReader.Timeout;

            // Initialize pools.
            m_DownloadTaskInfoSlotPool = m_RefPoolService.Add<DownloadTaskInfoSlot>(1024);
            m_DownloadTaskPool = m_RefPoolService.Add<DownloadTask>(m_ConcurrentDownloadCountLimit);
        }

        /// <summary>
        /// Start a downloading task.
        /// </summary>
        /// <param name="downloadTaskInfo">Downloading task info.</param>
        /// <returns>A unique ID of the downloading task.</returns>
        public int StartDownloading(DownloadTaskInfo downloadTaskInfo)
        {
            var slot = m_DownloadTaskInfoSlotPool.Acquire();
            slot.DownloadTaskInfo = downloadTaskInfo;
            slot.DownloadTaskId = ++m_CurrentDownloadTaskId;
            m_WaitingDownloadTaskInfoSlots[slot.DownloadTaskId] = slot;
            return slot.DownloadTaskId;
        }

        public bool StopDownloading(int taskId, bool quiet = false)
        {
            if (StopOngoingTask(taskId, quiet))
            {
                return true;
            }

            if (StopWaitingTask(taskId, quiet))
            {
                return true;
            }

            return false;
        }

        private bool StopWaitingTask(int taskId, bool quiet)
        {
            if (!m_WaitingDownloadTaskInfoSlots.TryGetValue(taskId, out DownloadTaskInfoSlot downloadTaskInfoSlot))
            {
                return false;
            }

            var downloadTaskInfo = downloadTaskInfoSlot.DownloadTaskInfo.Value;
            m_WaitingDownloadTaskInfoSlots.Remove(taskId);
            if (!quiet)
            {
                downloadTaskInfo.CallbackSet.OnFailure?.Invoke(taskId, downloadTaskInfo, DownloadErrorCode.StoppedByUser, string.Empty);
            }

            ReleaseDownloadTaskInfoSlot(downloadTaskInfoSlot);
            return true;
        }

        private bool StopOngoingTask(int taskId, bool quiet)
        {
            if (m_OngoingDownloadTasks.TryGetValue(taskId, out IDownloadTask downloadTask))
            {
                downloadTask.Stop();
                if (quiet)
                {
                    m_QuietlyStopTaskIds.Add(taskId);
                }

                return true;
            }

            return false;
        }

        private void InitConcurrentDownloadCountLimit()
        {
            var concurrentDownloadCountLimit = ConfigReader.ConcurrentDownloadCountLimit;
            if (concurrentDownloadCountLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(concurrentDownloadCountLimit));
            }

            m_ConcurrentDownloadCountLimit = concurrentDownloadCountLimit;
        }

        private void InitTempFileExtension()
        {
            var tempFileExtension = ConfigReader.TempFileExtension;
            if (string.IsNullOrEmpty(tempFileExtension))
            {
                throw new ArgumentException("Temp file extension is invalid.");
            }

            if (!tempFileExtension.StartsWith("."))
            {
                throw new ArgumentException("Temp file extension must start with a full stop.");
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                if (tempFileExtension.Contains(invalidChar))
                {
                    throw new ArgumentException("Temp file extension contains invalid characters.");
                }
            }

            m_TempFileExtension = tempFileExtension;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var kv in m_OngoingDownloadTasks)
                {
                    var task = kv.Value;
                    task.Stop();
                }

                m_OngoingDownloadTasks.Clear();
                m_DownloadTaskInfoSlotPool.Clear();
                m_DownloadTaskInfoSlotPool = null;
                m_DownloadTaskPool = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Generic tick method.
        /// </summary>
        /// <param name="timeStruct">Time struct.</param>
        protected override void OnUpdate(TimeStruct timeStruct)
        {
            m_DownloadTaskIdsToRemove.Clear();
            foreach (var kv in m_OngoingDownloadTasks)
            {
                int taskId = kv.Key;
                var task = kv.Value;
                var taskInfo = task.Info.Value;

                task.Update(timeStruct);

                if (task.IsDone)
                {
                    OnDownloadProgress(taskId, task, taskInfo);
                    taskInfo.CallbackSet.OnSuccess?.Invoke(taskId, taskInfo);
                    m_DownloadTaskIdsToRemove.Add(taskId);
                }
                else if (task.ErrorCode != null)
                {
                    if (!m_QuietlyStopTaskIds.Contains(taskId))
                    {
                        taskInfo.CallbackSet.OnFailure?.Invoke(taskId, taskInfo, task.ErrorCode.Value, task.ErrorMessage);
                    }
                    else
                    {
                        m_QuietlyStopTaskIds.Remove(taskId);
                    }

                    m_DownloadTaskIdsToRemove.Add(taskId);
                }
                else
                {
                    OnDownloadProgress(taskId, task, taskInfo);
                }
            }

            foreach (var taskId in m_DownloadTaskIdsToRemove)
            {
                var task = m_OngoingDownloadTasks[taskId];
                m_OngoingDownloadTasks.Remove(taskId);
                m_CachedOngoingDownloadedSizes.Remove(taskId);
                task.Reset();
                m_DownloadTaskPool.Release((DownloadTask)task);
            }

            while (m_OngoingDownloadTasks.Count < m_ConcurrentDownloadCountLimit && m_WaitingDownloadTaskInfoSlots.Count > 0)
            {
                var first = m_WaitingDownloadTaskInfoSlots.First();
                m_WaitingDownloadTaskInfoSlots.Remove(first.Key);
                var downloadTask = m_DownloadTaskPool.Acquire();
                downloadTask.DownloadTaskId = first.Value.DownloadTaskId;
                downloadTask.Info = first.Value.DownloadTaskInfo;
                downloadTask.DownloadService = this;
                downloadTask.Init();
                downloadTask.Start();
                m_OngoingDownloadTasks.Add(downloadTask.DownloadTaskId, downloadTask);
                m_CachedOngoingDownloadedSizes.Add(downloadTask.DownloadTaskId, downloadTask.DownloadedSize);
            }
        }

        private void OnDownloadProgress(int taskId, IDownloadTask task, DownloadTaskInfo taskInfo)
        {
            if (m_CachedOngoingDownloadedSizes[taskId] != task.DownloadedSize && taskInfo.CallbackSet.OnProgress != null)
            {
                taskInfo.CallbackSet.OnProgress(taskId, taskInfo, task.DownloadedSize);
                m_CachedOngoingDownloadedSizes[taskId] = task.DownloadedSize;
            }
        }

        private void ReleaseDownloadTaskInfoSlot(DownloadTaskInfoSlot slot)
        {
            slot.DownloadTaskId = 0;
            slot.DownloadTaskInfo = null;

            // Expand the capacity if needed.
            if (m_DownloadTaskInfoSlotPool.Count >= m_DownloadTaskInfoSlotPool.Capacity)
            {
                m_DownloadTaskInfoSlotPool.Capacity = m_DownloadTaskInfoSlotPool.Count * 2;
            }

            m_DownloadTaskInfoSlotPool.Release(slot);
        }
    }
}
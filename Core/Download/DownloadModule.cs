using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Default implementation of download module.
    /// </summary>
    public partial class DownloadModule : BaseModule, IDownloadModule
    {
        private IRefPoolModule m_RefPoolModule = null;

        /// <summary>
        /// Get or set the reference pool module.
        /// </summary>
        public IRefPoolModule RefPoolModule
        {
            get => m_RefPoolModule;

            set
            {
                if (m_RefPoolModule != null)
                {
                    throw new InvalidOperationException("Object pool module is already set.");
                }

                m_RefPoolModule = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        private string m_TempFileExtension = null;

        /// <summary>
        /// Temporary file extension, starting with a full stop.
        /// </summary>
        public string TempFileExtension
        {
            get => m_TempFileExtension ?? throw new InvalidOperationException("Temp file extension is not set.");

            set
            {
                if (m_TempFileExtension != null)
                {
                    throw new InvalidOperationException("Temp file extension can be set only once.");
                }

                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Temp file extension is invalid.");
                }

                if (!value.StartsWith("."))
                {
                    throw new ArgumentException("Temp file extension must start with a full stop.");
                }

                foreach (var invalidChar in Path.GetInvalidFileNameChars())
                {
                    if (value.Contains(invalidChar))
                    {
                        throw new ArgumentException("Temp file extension contains invalid characters.");
                    }
                }

                m_TempFileExtension = value;
            }
        }

        private int? m_ConcurrentDownloadCountLimit = null;

        /// <summary>
        /// The upper limit of the number of concurrent downloading tasks.
        /// </summary>
        public int ConcurrentDownloadCountLimit
        {
            get => m_ConcurrentDownloadCountLimit ?? throw new InvalidOperationException("Concurrent download count limit is not set.");

            set
            {
                if (m_ConcurrentDownloadCountLimit != null)
                {
                    throw new InvalidOperationException("Concurrent download count limit can be set only once.");
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                m_ConcurrentDownloadCountLimit = value;
            }
        }

        private int? m_ChunkSizeToSave = null;

        /// <summary>
        /// The chunk size in bytes to save to the disk. A value that is less than or equal to 0 means the download won't be chunk based.
        /// </summary>
        public int ChunkSizeToSave
        {
            get => m_ChunkSizeToSave ?? throw new InvalidOperationException("Not set.");

            set
            {
                if (m_ChunkSizeToSave != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                // A non-positive value means not to save in chunks.
                m_ChunkSizeToSave = value;
            }
        }

        private float? m_Timeout = null;

        /// <summary>
        /// Default time limit of any task.
        /// </summary>
        public float Timeout
        {
            get => m_Timeout ?? throw new InvalidOperationException("Not set.");

            set
            {
                if (m_Timeout != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                // A non-positive value, or Single.PositiveInfinity means not to have a timeout.
                m_Timeout = value;
            }
        }

        private IDownloadTaskPool m_DownloadTaskPool = null;

        /// <summary>
        /// Download task pool.
        /// </summary>
        public IDownloadTaskPool DownloadTaskPool
        {
            get { return m_DownloadTaskPool; }

            set { m_DownloadTaskPool = value; }
        }

        /// <summary>
        /// Factory for a <see cref="IDownloadTaskImpl"/> instance.
        /// </summary>
        public ISimpleFactory<IDownloadTaskImpl> DownloadTaskImplFactory { get; set; }

        private readonly SortedDictionary<int, DownloadTaskInfoSlot> m_WaitingDownloadTaskInfoSlots = new SortedDictionary<int, DownloadTaskInfoSlot>();

        private IRefPool<DownloadTaskInfoSlot> m_DownloadTaskInfoSlotPool = null;

        private readonly Dictionary<int, IDownloadTask> m_OngoingDownloadTasks = new Dictionary<int, IDownloadTask>();

        private readonly Dictionary<int, long> m_CachedOngoingDownloadedSizes = new Dictionary<int, long>();

        private readonly List<int> m_DownloadTaskIdsToRemove = new List<int>();

        private int m_CurrentDownloadTaskId = 0;

        private readonly HashSet<int> m_QuitelyStopTaskIds = new HashSet<int>();

        /// <summary>
        /// Start a downloading task.
        /// </summary>
        /// <param name="downloadTaskInfo">Downloading task info.</param>
        /// <returns>A unique ID of the downloading task.</returns>
        public int StartDownloading(DownloadTaskInfo downloadTaskInfo)
        {
            CheckStateOrThrow();
            var slot = m_DownloadTaskInfoSlotPool.Acquire();
            slot.DownloadTaskInfo = downloadTaskInfo;
            slot.DownloadTaskId = ++m_CurrentDownloadTaskId;
            m_WaitingDownloadTaskInfoSlots[slot.DownloadTaskId] = slot;
            return slot.DownloadTaskId;
        }

        public bool StopDownloading(int taskId, bool quiet = false)
        {
            CheckStateOrThrow();
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
                    m_QuitelyStopTaskIds.Add(taskId);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check whether the module is in an available state.
        /// </summary>
        protected internal override void CheckStateOrThrow()
        {
            base.CheckStateOrThrow();
            if (m_TempFileExtension == null)
            {
                throw new InvalidOperationException("Temp file extension is not set.");
            }

            if (m_ConcurrentDownloadCountLimit == null)
            {
                throw new InvalidOperationException("Concurrent download count limit is not set.");
            }

            if (m_ChunkSizeToSave == null)
            {
                throw new InvalidOperationException("Chunk size to save is not set.");
            }

            if (m_Timeout == null)
            {
                throw new InvalidOperationException("Timeout is not set.");
            }

            if (m_RefPoolModule == null)
            {
                throw new InvalidOperationException("Object pool module is not set.");
            }
        }

        /// <summary>
        /// Initialize this module.
        /// </summary>
        public override void Init()
        {
            base.Init();
            m_DownloadTaskInfoSlotPool = RefPoolModule.Add<DownloadTaskInfoSlot>(1024);
            m_DownloadTaskPool.Init();
        }

        /// <summary>
        /// Shut down this module.
        /// </summary>
        public override void ShutDown()
        {
            m_DownloadTaskInfoSlotPool.Clear();
            m_DownloadTaskInfoSlotPool = null;
            m_DownloadTaskPool.ShutDown();
            m_DownloadTaskPool = null;
            base.ShutDown();
        }

        /// <summary>
        /// Generic tick method.
        /// </summary>
        /// <param name="timeStruct">Time struct.</param>
        public override void Update(TimeStruct timeStruct)
        {
            base.Update(timeStruct);

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
                    if (!m_QuitelyStopTaskIds.Contains(taskId))
                    {
                        taskInfo.CallbackSet.OnFailure?.Invoke(taskId, taskInfo, task.ErrorCode.Value, task.ErrorMessage);
                    }
                    else
                    {
                        m_QuitelyStopTaskIds.Remove(taskId);
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
                DownloadTaskPool.Release(task);
            }

            while (m_OngoingDownloadTasks.Count < ConcurrentDownloadCountLimit && m_WaitingDownloadTaskInfoSlots.Count > 0)
            {
                var first = m_WaitingDownloadTaskInfoSlots.First();
                m_WaitingDownloadTaskInfoSlots.Remove(first.Key);
                var downloadTask = DownloadTaskPool.Acquire();
                downloadTask.DownloadTaskId = first.Value.DownloadTaskId;
                downloadTask.Info = first.Value.DownloadTaskInfo;
                downloadTask.DownloadModule = this;
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
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Default implementation of <see cref="IDownloadTask"/>.
    /// </summary>
    public sealed partial class DownloadTask : IDownloadTask
    {
        // Only set in worker threads for checking file sizes and check sums.
        // Therefore this is not a protection of downloading the same file twice simultaneously.
        private static readonly ConcurrentDictionary<string, bool> s_DownloadedFileBeingChecked
            = new ConcurrentDictionary<string, bool>();

        private enum Status
        {
            None,
            WaitingForSameNameChecking,
            Started,
            Checking,
            Finished,
        }

        private Status m_Status = Status.None;

        private Status ItsStatus
        {
            get => m_Status;
            set
            {
                if (value != m_Status)
                {
                    CoreLog.Debug($"[DownloadTask set_ItsStatus] ID: {DownloadTaskId}, State: {m_Status} --> {value}");
                }

                m_Status = value;
            }
        }

        private string m_TempSavePath = string.Empty;
        private long m_StartByteIndex = 0L;
        private long m_TempFileSize = 0L;
        private IDownloadTaskImpl m_DownloadTaskImpl;
        private FileStream m_FileStream;
        private long m_SizeToFlush = 0L;
        private CancellationTokenSource m_CheckingSubtaskCancellationTokenSource = null;
        private Task<CheckingSubtaskResult> m_CheckingSubtask = null;

        /// <summary>
        /// Download module this task is attached to.
        /// </summary>
        public int DownloadTaskId { get; set; }

        /// <summary>
        /// Download task info.
        /// </summary>
        public DownloadTaskInfo? Info { get; set; }

        /// <summary>
        /// Downloaded size in bytes.
        /// </summary>
        public long DownloadedSize { get; private set; }

        /// <summary>
        /// Error code.
        /// </summary>
        public DownloadErrorCode? ErrorCode { get; private set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Whether the current task is done successfully.
        /// </summary>
        public bool IsDone { get; private set; }

        /// <summary>
        /// Download module this task is attached to.
        /// </summary>
        public IDownloadModule DownloadModule { get; set; }

        /// <summary>
        /// Time used in seconds.
        /// </summary>
        public float TimeUsed { get; private set; }

        /// <summary>
        /// Initialize.
        /// </summary>
        public void Init()
        {
            if (m_DownloadTaskImpl == null)
            {
                m_DownloadTaskImpl = DownloadModule.DownloadTaskImplFactory.Get();
            }

            m_DownloadTaskImpl.ChunkSizeToSave = DownloadModule.ChunkSizeToSave;
        }

        /// <summary>
        /// Reset the task.
        /// </summary>
        public void Reset()
        {
            ItsStatus = Status.None;
            DownloadTaskId = 0;
            Info = null;
            ErrorCode = null;
            ErrorMessage = string.Empty;

            ClearCheckingSubtask();

            DownloadedSize = 0L;
            IsDone = false;
            DownloadModule = null;
            TimeUsed = 0f;
            m_StartByteIndex = 0L;
            m_TempFileSize = 0L;
            m_TempSavePath = string.Empty;
            m_SizeToFlush = 0L;
            m_DownloadTaskImpl.OnReset();
            ClearFileStreamIfNeeded();
        }

        private void ClearCheckingSubtask()
        {
            if (m_CheckingSubtaskCancellationTokenSource != null)
            {
                if (!m_CheckingSubtaskCancellationTokenSource.IsCancellationRequested)
                {
                    m_CheckingSubtaskCancellationTokenSource.Cancel();
                }

                m_CheckingSubtaskCancellationTokenSource.Dispose();
                m_CheckingSubtaskCancellationTokenSource = null;
                m_CheckingSubtask = null;
            }
        }

        private void ClearFileStreamIfNeeded()
        {
            if (m_FileStream == null) return;
            m_FileStream.Dispose();
            m_FileStream = null;
        }

        /// <summary>
        /// Start the task.
        /// </summary>
        public void Start()
        {
            if (ItsStatus != Status.None)
            {
                throw new InvalidOperationException(Utility.Text.Format("Cannot start in status '{0}'", ItsStatus));
            }

            if (DownloadModule == null)
            {
                throw new InvalidOperationException("Download module is invalid.");
            }

            if (Info == null)
            {
                throw new InvalidOperationException("Download task info is invalid.");
            }

            m_TempSavePath = Info.Value.SavePath + DownloadModule.TempFileExtension;
            if (File.Exists(m_TempSavePath))
            {
                m_TempFileSize = m_StartByteIndex = new FileInfo(m_TempSavePath).Length;
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(m_TempSavePath) ?? throw new NullReferenceException());
                m_TempFileSize = m_StartByteIndex = 0L;
            }

            var taskInfo = Info.Value;
            if (s_DownloadedFileBeingChecked.ContainsKey(m_TempSavePath))
            {
                ItsStatus = Status.WaitingForSameNameChecking;
            }
            else
            {
                CheckOrStart(taskInfo);
            }
        }

        private void CheckOrStart(DownloadTaskInfo taskInfo)
        {
            if (taskInfo.Size > 0L && taskInfo.Size <= m_TempFileSize)
            {
                DownloadedSize = m_TempFileSize;
                TackleDownloadingIsOver(taskInfo);
            }
            else
            {
                SwitchToStartedStatus(taskInfo);
            }
        }

        private void SwitchToStartedStatus(DownloadTaskInfo taskInfo)
        {
            m_FileStream = File.Open(m_TempSavePath, FileMode.Append);
            ItsStatus = Status.Started;
            m_DownloadTaskImpl.OnStart(taskInfo.UrlStr, m_StartByteIndex);
        }

        /// <summary>
        /// Stop the task.
        /// </summary>
        public void Stop()
        {
            if (ItsStatus == Status.None)
            {
                throw new InvalidOperationException(Utility.Text.Format("Cannot stop in status '{0}'", ItsStatus));
            }

            if (ItsStatus != Status.Finished)
            {
                switch (ItsStatus)
                {
                    case Status.Started:
                        m_DownloadTaskImpl.OnStop();
                        IsDone = false;
                        break;
                    case Status.WaitingForSameNameChecking:
                        m_CheckingSubtaskCancellationTokenSource.Cancel();
                        m_CheckingSubtaskCancellationTokenSource.Dispose();
                        m_CheckingSubtaskCancellationTokenSource = null;
                        break;
                    case Status.Checking:
                        break;
                }

                IsDone = false;
                ItsStatus = Status.Finished;
                ErrorCode = DownloadErrorCode.StoppedByUser;
            }

            ClearFileStreamIfNeeded();
        }

        /// <summary>
        /// Generic tick method.
        /// </summary>
        /// <param name="timeStruct">Time struct.</param>
        public void Update(TimeStruct timeStruct)
        {
            if (ItsStatus == Status.None)
            {
                throw new InvalidOperationException(Utility.Text.Format("Cannot tick in status '{0}'", ItsStatus));
            }

            if (ItsStatus == Status.Finished)
            {
                return;
            }

            var taskInfo = Info.Value;
            if (ItsStatus == Status.WaitingForSameNameChecking)
            {
                if (!s_DownloadedFileBeingChecked.ContainsKey(m_TempSavePath))
                {
                    CheckOrStart(taskInfo);
                }

                return;
            }

            if (ItsStatus == Status.Checking)
            {
                UpdateChecking(taskInfo);
                return;
            }

            m_DownloadTaskImpl.Update(timeStruct);

            TimeUsed += timeStruct.UnscaledDeltaTime;
            DownloadedSize = m_StartByteIndex + m_DownloadTaskImpl.RealDownloadedSize;

            if (DownloadModule.Timeout > 0 && TimeUsed > DownloadModule.Timeout)
            {
                ClearFileStreamIfNeeded();
                TackleTimeOut();
                return;
            }

            if (m_DownloadTaskImpl.ErrorCode != null)
            {
                ClearFileStreamIfNeeded();
                TackleWebRequestError();
                return;
            }

            if (m_DownloadTaskImpl.IsDone)
            {
                SaveDownloadedDataToFile(true);
                ClearFileStreamIfNeeded();
                TackleDownloadingIsOver(taskInfo);
                return;
            }

            SaveDownloadedDataToFile(false);
        }

        private void UpdateChecking(DownloadTaskInfo taskInfo)
        {
            if (m_CheckingSubtask.IsFaulted)
            {
                foreach (var e in m_CheckingSubtask.Exception.InnerExceptions)
                {
                    throw e;
                }
            }
            else if (m_CheckingSubtask.IsCompleted)
            {
                var result = m_CheckingSubtask.Result;
                if (result.ErrorCode == null)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(taskInfo.SavePath) ?? throw new NullReferenceException());
                    if (File.Exists(taskInfo.SavePath))
                    {
                        File.Delete(taskInfo.SavePath);
                    }

                    File.Move(m_TempSavePath, taskInfo.SavePath);
                    IsDone = true;
                    ErrorCode = null;
                }
                else
                {
                    IsDone = false;
                    ErrorCode = result.ErrorCode;
                    ErrorMessage = result.ErrorMessage;
                    File.Delete(m_TempSavePath);
                }

                ItsStatus = Status.Finished;
            }
        }

        private void TackleDownloadingIsOver(DownloadTaskInfo taskInfo)
        {
            ItsStatus = Status.Checking;
            m_CheckingSubtaskCancellationTokenSource = new CancellationTokenSource();
            m_CheckingSubtask = Task.Run(() =>
                    CheckDownloadedFile(taskInfo, m_TempSavePath, m_CheckingSubtaskCancellationTokenSource.Token),
                m_CheckingSubtaskCancellationTokenSource.Token);
        }

        private CheckingSubtaskResult CheckDownloadedFile(DownloadTaskInfo taskInfo, string tempSavePath, CancellationToken cancellationToken)
        {
            var keyAdded = s_DownloadedFileBeingChecked.TryAdd(m_TempSavePath, true);
            if (!keyAdded)
            {
                throw new InvalidOperationException("Oops. Cannot add temp save path to dictionary.");
            }

            try
            {
                bool sizeCheck = taskInfo.Size <= 0L || taskInfo.Size == DownloadedSize;
                var errorMessage = string.Empty;

                if (!sizeCheck)
                {
                    errorMessage = Utility.Text.Format("Expected size is '{0}' while actual size is '{1}'", taskInfo.Size, DownloadedSize);
                    return new CheckingSubtaskResult
                    {
                        ErrorCode = DownloadErrorCode.WrongSize,
                        ErrorMessage = errorMessage
                    };
                }

                if (taskInfo.Crc32 == null || cancellationToken.IsCancellationRequested)
                {
                    return default(CheckingSubtaskResult);
                }

                using (var fs = File.OpenRead(m_TempSavePath))
                {
                    var actualCrc32 = Algorithm.Crc32.Sum(fs);
                    if (actualCrc32 == taskInfo.Crc32.Value)
                    {
                        return default(CheckingSubtaskResult);
                    }

                    errorMessage = Utility.Text.Format("CRC 32 inconsistency: expects '{0}' but actually is '{1}'.",
                        taskInfo.Crc32.Value, actualCrc32);
                    return new CheckingSubtaskResult
                    {
                        ErrorCode = DownloadErrorCode.WrongChecksum,
                        ErrorMessage = errorMessage
                    };
                }
            }
            finally
            {
                s_DownloadedFileBeingChecked.TryRemove(tempSavePath, out _);
            }
        }

        private void TackleWebRequestError()
        {
            ErrorCode = m_DownloadTaskImpl.ErrorCode;
            ErrorMessage = m_DownloadTaskImpl.ErrorMessage;
            ItsStatus = Status.Finished;
        }

        private void TackleTimeOut()
        {
            ErrorCode = DownloadErrorCode.Timeout;
            ErrorMessage = string.Empty;
            m_DownloadTaskImpl.OnTimeOut();
            ItsStatus = Status.Finished;
        }

        private void SaveDownloadedDataToFile(bool forceFlush)
        {
            long startIndex = m_TempFileSize + m_SizeToFlush - m_StartByteIndex;
            long sizeToWrite = m_DownloadTaskImpl.RealDownloadedSize - startIndex;

            if (sizeToWrite <= 0L)
            {
                return;
            }

            m_SizeToFlush += sizeToWrite;

#if PROFILING
            Profiler.BeginSample();
#endif
            try
            {
                m_DownloadTaskImpl.WriteDownloadedContent(m_FileStream, startIndex, sizeToWrite);
                if (forceFlush || DownloadModule.ChunkSizeToSave > 0 && m_SizeToFlush >= DownloadModule.ChunkSizeToSave)
                {
                    m_FileStream.Flush();
                    m_TempFileSize += m_SizeToFlush;
                    m_SizeToFlush = 0;
                }
            }
            catch (IOException e)
            {
                ErrorCode = DownloadErrorCode.FileIOException;
                ErrorMessage = e.Message;
            }
            finally
            {
#if PROFILING
                CoreLog.Debug($"[DownloadTask SaveDownloadedDataToFile] Writing {sizeToWrite} bytes of file takes time {Profiler.EndSample().TotalMilliseconds} ms.");
#endif
            }
        }
    }
}
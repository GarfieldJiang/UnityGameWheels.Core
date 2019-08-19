using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Default implementation of <see cref="IDownloadTask"/>.
    /// </summary>
    public sealed class DownloadTask : IDownloadTask
    {
        private enum Status
        {
            None,
            Started,
            Finished,
        }

        private Status m_Status = Status.None;
        private string m_TempSavePath = string.Empty;
        private long m_StartByteIndex = 0L;
        private long m_TempFileSize = 0L;
        private IDownloadTaskImpl m_DownloadTaskImpl;
        private FileStream m_FileStream;
        private long m_SizeToFlush = 0L;

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
            m_Status = Status.None;
            DownloadTaskId = 0;
            Info = null;
            ErrorCode = null;
            ErrorMessage = string.Empty;
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
            if (m_Status != Status.None)
            {
                throw new InvalidOperationException(Utility.Text.Format("Cannot start in status '{0}'", m_Status));
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
            if (taskInfo.Size > 0L && taskInfo.Size <= m_TempFileSize)
            {
                m_Status = Status.Finished;
                DownloadedSize = m_TempFileSize;
                TackleDownloadingIsOver(ref taskInfo);
            }
            else
            {
                m_FileStream = File.Open(m_TempSavePath, FileMode.Append);
                m_Status = Status.Started;
                m_DownloadTaskImpl.OnStart(taskInfo.UrlStr, m_StartByteIndex);
            }
        }

        /// <summary>
        /// Stop the task.
        /// </summary>
        public void Stop()
        {
            if (m_Status == Status.None)
            {
                throw new InvalidOperationException(Utility.Text.Format("Cannot stop in status '{0}'", m_Status));
            }

            if (m_Status == Status.Started)
            {
                m_DownloadTaskImpl.OnStop();
                IsDone = false;
                m_Status = Status.Finished;
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
            if (m_Status == Status.None)
            {
                throw new InvalidOperationException(Utility.Text.Format("Cannot tick in status '{0}'", m_Status));
            }

            if (m_Status == Status.Finished)
            {
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

            var taskInfo = Info.Value;
            if (m_DownloadTaskImpl.IsDone)
            {
                SaveDownloadedDataToFile(true);
                ClearFileStreamIfNeeded();
                TackleDownloadingIsOver(ref taskInfo);
                return;
            }

            SaveDownloadedDataToFile(false);
        }

        private void TackleDownloadingIsOver(ref DownloadTaskInfo taskInfo)
        {
            var errorCode = CheckDownloadedFile(ref taskInfo, out var errorMessage);
            if (errorCode == null)
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
                ErrorCode = errorCode;
                ErrorMessage = errorMessage;
                File.Delete(m_TempSavePath);
            }

            m_Status = Status.Finished;
        }

        private DownloadErrorCode? CheckDownloadedFile(ref DownloadTaskInfo taskInfo, out string errorMessage)
        {
            bool sizeCheck = taskInfo.Size <= 0L || taskInfo.Size == DownloadedSize;
            errorMessage = string.Empty;

            if (!sizeCheck)
            {
                errorMessage = Utility.Text.Format("Expected size is '{0}' while actual size is '{1}'", taskInfo.Size, DownloadedSize);
                return DownloadErrorCode.WrongSize;
            }

            if (taskInfo.Crc32 == null)
            {
                return null;
            }

            using (var fs = File.OpenRead(m_TempSavePath))
            {
                var actualCrc32 = Algorithm.Crc32.Sum(fs);
                if (actualCrc32 == taskInfo.Crc32.Value)
                {
                    return null;
                }
                else
                {
                    errorMessage = Utility.Text.Format("CRC 32 inconsistency: expects '{0}' but actually is '{1}'.",
                        taskInfo.Crc32.Value, actualCrc32);
                    return DownloadErrorCode.WrongChecksum;
                }
            }
        }

        private void TackleWebRequestError()
        {
            ErrorCode = m_DownloadTaskImpl.ErrorCode;
            ErrorMessage = m_DownloadTaskImpl.ErrorMessage;
            m_Status = Status.Finished;
        }

        private void TackleTimeOut()
        {
            ErrorCode = DownloadErrorCode.Timeout;
            ErrorMessage = string.Empty;
            m_DownloadTaskImpl.OnTimeOut();
            m_Status = Status.Finished;
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
using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class DownloadTaskTests
    {
        private ITickService m_TickService = null;
        private IDownloadService m_DownloadService = null;
        private IRefPoolService m_RefPoolService = null;
        private ISimpleFactory<IDownloadTaskImpl> m_DownloadTaskImplFactory = null;
        private DirectoryInfo m_DirectoryInfo = null;

        private static string SavePathRoot => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                              + Path.DirectorySeparatorChar + "DownloadTest";

        private class MockDownloadTaskImplFactory : ISimpleFactory<IDownloadTaskImpl>
        {
            public long TaskSize { get; set; }

            public float TaskTimeNeeded { get; set; }

            public bool TaskShouldNeverStart { get; set; }

            public bool DontMakeProgress { get; set; }

            public IDownloadTaskImpl Get()
            {
                var ret = new MockDownloadTaskImpl();
                ret.DontMakeProgress = DontMakeProgress;
                ret.Size = TaskSize;
                ret.TimeNeeded = TaskTimeNeeded;
                ret.ShouldNeverStart = TaskShouldNeverStart;
                return ret;
            }
        }

        private class MockDownloadTaskImpl : IDownloadTaskImpl
        {
            public long Size { get; set; }

            public float TimeNeeded { get; set; }

            public bool IsDone { get; private set; }

            public long RealDownloadedSize { get; private set; }

            public bool ShouldNeverStart { get; set; }

            public DownloadErrorCode? ErrorCode => null;

            public string ErrorMessage { get; private set; }

            public int ChunkSizeToSave { get; set; }

            private long m_StartByteIndex = 0L;
            private float m_TimeUsed = 0f;

            public bool DontMakeProgress { get; set; }

            public void OnDownloadError()
            {
                // Empty.
            }

            public void OnReset()
            {
                // Empty.
            }

            public void OnStart(string urlStr, long startByteIndex)
            {
                if (ShouldNeverStart)
                {
                    throw new InvalidOperationException("Trying to start a task that should never start.");
                }

                m_StartByteIndex = startByteIndex;
                m_TimeUsed = 0f;
            }

            public void OnStop()
            {
                // Empty.
            }

            public void OnTimeOut()
            {
                // Empty.
            }

            public void Update(TimeStruct timeStruct)
            {
                if (IsDone)
                {
                    return;
                }

                if (DontMakeProgress)
                {
                    return;
                }

                m_TimeUsed += timeStruct.UnscaledDeltaTime;
                RealDownloadedSize = m_StartByteIndex + (long)(m_TimeUsed * Size / TimeNeeded);
                if (RealDownloadedSize + m_StartByteIndex > Size)
                {
                    RealDownloadedSize = Size - m_StartByteIndex;
                    IsDone = true;
                }
            }

            public void WriteDownloadedContent(Stream stream, long offset, long size)
            {
                var buffer = new byte[size];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)((offset + m_StartByteIndex + i) % (byte.MaxValue + 1));
                }

                stream.Write(buffer, 0, buffer.Length);
            }
        }

        [Test]
        public void TestSimpleDownload()
        {
            string fileName = "simple_file";
            long size = 100L;
            float timeNeeded = 1f;
            string savePath = Path.Combine(m_DirectoryInfo.FullName, fileName);
            var mockDownloadTaskImplFactory = m_DownloadTaskImplFactory as MockDownloadTaskImplFactory;
            mockDownloadTaskImplFactory.TaskSize = size;
            mockDownloadTaskImplFactory.TaskTimeNeeded = timeNeeded;
            int successCount = 0;
            int failureCount = 0;

            m_DownloadService.StartDownloading(new DownloadTaskInfo(
                urlStr: "urlStr",
                savePath: savePath,
                size: size,
                crc32: null,
                callbackSet: new DownloadCallbackSet
                {
                    OnSuccess = (taskId, taskInfo) => { ++successCount; },
                    OnFailure = (taskId, taskInfo, errorCode, errorMessage) => { ++failureCount; }
                },
                context: null
            ));

            for (float time = 0f; time < timeNeeded + 0.5f; time += 0.1f)
            {
                ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(.1f, .1f, time, time));
                Thread.Sleep(100);
            }

            Assert.AreEqual(0, failureCount);
            Assert.AreEqual(1, successCount);
            Assert.IsTrue(File.Exists(savePath));
            var bytes = File.ReadAllBytes(savePath);
            CheckBufferContent(size, bytes);
        }

        [Test]
        public void TestDownloadTimeout()
        {
            string fileName = "simple_file";
            long size = 100L;
            float timeNeeded = m_DownloadService.Timeout * 10;
            string savePath = Path.Combine(m_DirectoryInfo.FullName, fileName);
            var mockDownloadTaskImplFactory = m_DownloadTaskImplFactory as MockDownloadTaskImplFactory;
            mockDownloadTaskImplFactory.TaskSize = size;
            mockDownloadTaskImplFactory.TaskTimeNeeded = timeNeeded;
            DownloadErrorCode? downloadErrorCode = null;

            mockDownloadTaskImplFactory.DontMakeProgress = true;
            m_DownloadService.StartDownloading(new DownloadTaskInfo(
                urlStr: "urlStr",
                savePath: savePath,
                size: size,
                crc32: null,
                callbackSet: new DownloadCallbackSet
                {
                    OnSuccess = (taskId, taskInfo) => { },
                    OnFailure = (taskId, taskInfo, errorCode, errorMessage) => { downloadErrorCode = errorCode; },
                },
                context: null
            ));

            var oldTime = 0f;
            for (float time = oldTime; time < timeNeeded + 0.5f; time += m_DownloadService.Timeout + .1f)
            {
                var deltaTime = time - oldTime;
                oldTime = time;
                ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(deltaTime, deltaTime, time, time));
            }

            mockDownloadTaskImplFactory.DontMakeProgress = false;
            Assert.True(downloadErrorCode != null && downloadErrorCode.Value == DownloadErrorCode.Timeout);
        }

        [Test]
        public void TestContinueWithLastDownload()
        {
            string fileName = "simple_file";
            long size = 100L;
            long alreadyDownloadedSize = 35L;
            float timeNeeded = 1f;
            string savePath = Path.Combine(m_DirectoryInfo.FullName, fileName);
            string tempSavePath = savePath + m_DownloadService.TempFileExtension;

            // Fake already downloaded file.
            File.Create(tempSavePath).Close();
            File.WriteAllBytes(tempSavePath, CreateBufferContent(alreadyDownloadedSize));

            (m_DownloadTaskImplFactory as MockDownloadTaskImplFactory).TaskSize = size;
            (m_DownloadTaskImplFactory as MockDownloadTaskImplFactory).TaskTimeNeeded = timeNeeded;

            int successCount = 0;
            int failureCount = 0;

            m_DownloadService.StartDownloading(new DownloadTaskInfo(
                urlStr: "urlStr",
                savePath: savePath,
                size: size,
                crc32: null,
                callbackSet: new DownloadCallbackSet
                {
                    OnSuccess = (taskId, taskInfo) => { ++successCount; },
                    OnFailure = (taskId, taskInfo, errorCode, errorMessage) => { ++failureCount; },
                },
                context: null
            ));

            for (float time = 0f; time < timeNeeded + 0.5f; time += 0.1f)
            {
                ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(.1f, .1f, time, time));
                Thread.Sleep(100);
            }

            Assert.AreEqual(0, failureCount);
            Assert.AreEqual(1, successCount);
            Assert.IsTrue(File.Exists(savePath));
            var bytes = File.ReadAllBytes(savePath);
            CheckBufferContent(size, bytes);
        }

        [Test]
        public void TestContinueWithAlreadyDownloadedFile()
        {
            string fileName = "simple_file";
            long size = 100L;
            long alreadyDownloadedSize = size;
            float timeNeeded = 1f;
            string savePath = Path.Combine(m_DirectoryInfo.FullName, fileName);
            string tempSavePath = savePath + m_DownloadService.TempFileExtension;

            // Fake already downloaded file.
            File.Create(tempSavePath).Close();
            File.WriteAllBytes(tempSavePath, CreateBufferContent(alreadyDownloadedSize));
            var mockDownloadTaskImplFactory = m_DownloadTaskImplFactory as MockDownloadTaskImplFactory;
            mockDownloadTaskImplFactory.TaskShouldNeverStart = true;
            mockDownloadTaskImplFactory.TaskSize = size;
            mockDownloadTaskImplFactory.TaskTimeNeeded = timeNeeded;

            int successCount = 0;
            int failureCount = 0;

            m_DownloadService.StartDownloading(new DownloadTaskInfo(
                urlStr: "urlStr",
                savePath: savePath,
                size: size,
                crc32: null,
                callbackSet: new DownloadCallbackSet
                {
                    OnSuccess = (taskId, taskInfo) => { ++successCount; },
                    OnFailure = (taskId, taskInfo, errorCode, errorMessage) => { ++failureCount; },
                },
                context: null
            ));

            for (float time = 0f; time < timeNeeded + 0.5f; time += 0.1f)
            {
                ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(.1f, .1f, time, time));
                Thread.Sleep(100);
            }

            Assert.AreEqual(0, failureCount);
            Assert.AreEqual(1, successCount);
            Assert.IsTrue(File.Exists(savePath));
            var bytes = File.ReadAllBytes(savePath);
            CheckBufferContent(size, bytes);
        }

        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(SavePathRoot))
            {
                Directory.Delete(SavePathRoot, true);
            }

            if (!Directory.Exists(SavePathRoot))
            {
                Directory.CreateDirectory(SavePathRoot);
            }

            m_DirectoryInfo = new DirectoryInfo(SavePathRoot);

            m_TickService = new MockTickService();
            m_RefPoolService = new MockRefPoolService();
            m_DownloadService = new DownloadService { TickService = m_TickService, TickOrder = 0 };
            var configReader = Substitute.For<IDownloadServiceConfigReader>();
            configReader.TempFileExtension.Returns(".tmp");
            configReader.Timeout.Returns(1f);
            configReader.ChunkSizeToSave.Returns(32);
            configReader.ConcurrentDownloadCountLimit.Returns(2);
            m_DownloadService.ConfigReader = configReader;

            m_DownloadTaskImplFactory = new MockDownloadTaskImplFactory();
            var mockDownloadTaskImplFactory = m_DownloadTaskImplFactory as MockDownloadTaskImplFactory;
            mockDownloadTaskImplFactory.TaskShouldNeverStart = false;

            m_DownloadService.RefPoolService = m_RefPoolService;
            m_DownloadService.DownloadTaskImplFactory = m_DownloadTaskImplFactory;
            m_DownloadService.OnInit();
        }

        [TearDown]
        public void TearDown()
        {
            m_DownloadService.OnShutdown();
            m_DownloadService = null;
            m_RefPoolService = null;
            m_TickService = null;
            m_DownloadTaskImplFactory = null;
            m_DirectoryInfo = null;
            if (Directory.Exists(SavePathRoot))
            {
                Directory.Delete(SavePathRoot, true);
            }
        }

        private static void CheckBufferContent(long size, byte[] bytes)
        {
            Assert.AreEqual(size, bytes.LongLength);
            for (int i = 0; i < (int)size; i++)
            {
                Assert.AreEqual((byte)(i % (byte.MaxValue + 1)), bytes[i]);
            }
        }

        private static byte[] CreateBufferContent(long size)
        {
            var ret = new byte[size];
            for (int i = 0; i < (int)size; i++)
            {
                ret[i] = (byte)(i % (byte.MaxValue + 1));
            }

            return ret;
        }
    }
}
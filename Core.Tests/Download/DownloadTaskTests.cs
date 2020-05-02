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
        private IDownloadModule m_DownloadModule = null;
        private IRefPoolService m_ObjectPoolService = null;
        private IDownloadTaskPool m_DownloadTaskPool = null;
        private ISimpleFactory<IDownloadTaskImpl> m_DownloadTaskImplFactory = null;
        private DirectoryInfo m_DirectoryInfo = null;

        private static string SavePathRoot
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "DownloadTest"; }
        }

        private class MockDownloadTaskImplFactory : ISimpleFactory<IDownloadTaskImpl>
        {
            public long TaskSize { get; set; }

            public float TaskTimeNeeded { get; set; }

            public bool TaskShouldNeverStart { get; set; }

            public IDownloadTaskImpl Get()
            {
                var ret = new MockDownloadTaskImpl();
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

            m_DownloadModule.StartDownloading(new DownloadTaskInfo(
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
                m_DownloadModule.Update(new TimeStruct(.1f, .1f, time, time));
                Thread.Sleep(100);
            }

            Assert.AreEqual(0, failureCount);
            Assert.AreEqual(1, successCount);
            Assert.IsTrue(File.Exists(savePath));
            var bytes = File.ReadAllBytes(savePath);
            CheckBufferContent(size, bytes);
        }

        [Test]
        public void TestContinueWithLastDownload()
        {
            string fileName = "simple_file";
            long size = 100L;
            long alreadyDownloadedSize = 35L;
            float timeNeeded = 1f;
            string savePath = Path.Combine(m_DirectoryInfo.FullName, fileName);
            string tempSavePath = savePath + m_DownloadModule.TempFileExtension;

            // Fake already downloaded file.
            File.Create(tempSavePath).Close();
            File.WriteAllBytes(tempSavePath, CreateBufferContent(alreadyDownloadedSize));

            (m_DownloadTaskImplFactory as MockDownloadTaskImplFactory).TaskSize = size;
            (m_DownloadTaskImplFactory as MockDownloadTaskImplFactory).TaskTimeNeeded = timeNeeded;

            int successCount = 0;
            int failureCount = 0;

            m_DownloadModule.StartDownloading(new DownloadTaskInfo(
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
                m_DownloadModule.Update(new TimeStruct(.1f, .1f, time, time));
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
            string tempSavePath = savePath + m_DownloadModule.TempFileExtension;

            // Fake already downloaded file.
            File.Create(tempSavePath).Close();
            File.WriteAllBytes(tempSavePath, CreateBufferContent(alreadyDownloadedSize));
            var mockDownloadTaskImplFactory = m_DownloadTaskImplFactory as MockDownloadTaskImplFactory;
            mockDownloadTaskImplFactory.TaskShouldNeverStart = true;
            mockDownloadTaskImplFactory.TaskSize = size;
            mockDownloadTaskImplFactory.TaskTimeNeeded = timeNeeded;

            int successCount = 0;
            int failureCount = 0;

            m_DownloadModule.StartDownloading(new DownloadTaskInfo(
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
                m_DownloadModule.Update(new TimeStruct(.1f, .1f, time, time));
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

            m_ObjectPoolService = new RefPoolService();
            m_ObjectPoolService.OnInit();

            m_DownloadModule = new DownloadModule();
            m_DownloadModule.ChunkSizeToSave = 32;
            m_DownloadModule.ConcurrentDownloadCountLimit = 2;
            m_DownloadModule.TempFileExtension = ".tmp";
            m_DownloadModule.Timeout = 10000f;

            m_DownloadTaskPool = Substitute.For<IDownloadTaskPool>();
            m_DownloadTaskPool.Acquire().Returns(callInfo => new DownloadTask());

            m_DownloadTaskImplFactory = new MockDownloadTaskImplFactory();
            var mockDownloadTaskImplFactory = m_DownloadTaskImplFactory as MockDownloadTaskImplFactory;
            mockDownloadTaskImplFactory.TaskShouldNeverStart = false;

            m_DownloadModule.DownloadTaskPool = m_DownloadTaskPool;
            m_DownloadModule.RefPoolService = m_ObjectPoolService;
            m_DownloadModule.DownloadTaskImplFactory = m_DownloadTaskImplFactory;
            m_DownloadModule.Init();
        }

        [TearDown]
        public void TearDown()
        {
            m_DownloadTaskPool = null;
            m_DownloadModule.ShutDown();
            m_DownloadModule = null;
            m_ObjectPoolService.OnShutdown();
            m_ObjectPoolService = null;
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
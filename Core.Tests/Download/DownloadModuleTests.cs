using NSubstitute;
using NUnit.Framework;
using System;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class DownloadModuleTests
    {
        private IDownloadModule m_DownloadModule = null;
        private IRefPoolService m_ObjectPoolService = null;
        private IDownloadTaskPool m_DownloadTaskPool = null;

        private abstract class MockDownloadTask : IDownloadTask
        {
            public MockDownloadTask(float unscaledTimeNeeded)
            {
                m_UnscaledTimeNeeded = unscaledTimeNeeded;
            }

            protected float m_UnscaledTimeNeeded = 0f;

            public int DownloadTaskId { get; set; }

            public DownloadTaskInfo? Info { get; set; }

            public long DownloadedSize { get; protected set; }

            public DownloadErrorCode? ErrorCode { get; protected set; }

            public string ErrorMessage { get; protected set; }

            public bool IsDone { get; protected set; }

            public float TimeUsed { get; protected set; }

            public IDownloadModule DownloadModule { get; set; }

            public void Reset()
            {
                m_UnscaledTimeNeeded = 0f;
                DownloadTaskId = 0;
                Info = null;
                DownloadedSize = 0;
                ErrorCode = null;
                ErrorMessage = string.Empty;
                IsDone = false;
            }

            public void Start()
            {
                // Empty.
            }

            public virtual void Stop()
            {
                // Empty.
            }

            public void Init()
            {
                // Empty.
            }

            public abstract void Update(TimeStruct timeStruct);
        }

        private class MockSuccessfulDownloadTask : MockDownloadTask
        {
            public const float ChangeProgressTimeInterval = 1f;
            private float m_LastChangeProgressTime = 0f;

            public MockSuccessfulDownloadTask(float unscaledTimeNeeded) : base(unscaledTimeNeeded)
            {
                // Empty.
            }

            public override void Update(TimeStruct timeStruct)
            {
                TimeUsed += timeStruct.UnscaledDeltaTime;
                if (TimeUsed > DownloadModule.Timeout)
                {
                    ErrorCode = DownloadErrorCode.Timeout;
                    return;
                }

                if (TimeUsed > m_UnscaledTimeNeeded)
                {
                    IsDone = true;
                    return;
                }

                if (TimeUsed - m_LastChangeProgressTime > ChangeProgressTimeInterval)
                {
                    DownloadedSize = DownloadedSize + 1L;
                    m_LastChangeProgressTime = TimeUsed;
                }
            }

            public override void Stop()
            {
                ErrorCode = DownloadErrorCode.StoppedByUser;
            }
        }

        private class MockFailureDownloadTask : MockDownloadTask
        {
            public MockFailureDownloadTask(float unscaledTimeNeeded) : base(unscaledTimeNeeded)
            {
                // Empty.
            }

            public override void Update(TimeStruct timeStruct)
            {
                TimeUsed += timeStruct.UnscaledDeltaTime;
                if (TimeUsed > m_UnscaledTimeNeeded)
                {
                    ErrorCode = DownloadErrorCode.Unknown;
                }
            }
        }

        private class MockLengthAndCheckSumCheckingDownloadTask : MockDownloadTask
        {
            public MockLengthAndCheckSumCheckingDownloadTask(float unscaledTimeNeeded, long size)
                : base(unscaledTimeNeeded)
            {
                DownloadedSize = size;
            }

            public override void Update(TimeStruct timeStruct)
            {
                TimeUsed += timeStruct.UnscaledDeltaTime;
                if (TimeUsed > m_UnscaledTimeNeeded)
                {
                    if (Info.Value.Size > 0 && Info.Value.Size != DownloadedSize)
                    {
                        ErrorCode = DownloadErrorCode.WrongSize;
                    }
                    else
                    {
                        IsDone = true;
                    }
                }
            }
        }

        [Test]
        public void TestSimpleSuccessfulDownload()
        {
            m_DownloadTaskPool.Acquire().Returns(anyCallInfo => { return new MockSuccessfulDownloadTask(1f); });

            int successCount = 0;
            int failureCount = 0;

            int taskId = m_DownloadModule.StartDownloading(new DownloadTaskInfo("url", "savePath",
                new DownloadCallbackSet
                {
                    OnSuccess = (theTaskId, taskInfo) => { ++successCount; },
                    OnFailure = (theTaskId, taskInfo, errorCode, errorMessage) => { ++failureCount; }
                }));

            Assert.AreEqual(0, successCount);
            Assert.AreEqual(0, failureCount);

            m_DownloadModule.Update(new TimeStruct(0f, 0f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(1.1f, 1.1f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(.1f, 0f, 0f, 0f));

            Assert.AreEqual(1, successCount);
            Assert.AreEqual(0, failureCount);
        }

        [Test]
        public void TestStopOngoingTask()
        {
            m_DownloadTaskPool.Acquire().Returns(anyCallInfo => { return new MockSuccessfulDownloadTask(1f); });

            int successCount = 0;
            int failureCount = 0;

            int taskId = m_DownloadModule.StartDownloading(new DownloadTaskInfo("url", "savePath", new DownloadCallbackSet
            {
                OnSuccess = (theTaskId, taskInfo) => { ++successCount; },
                OnFailure = (theTaskId, taskInfo, errorCode, errorMessage) =>
                {
                    ++failureCount;
                    Assert.AreEqual(DownloadErrorCode.StoppedByUser, errorCode);
                }
            }));

            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));

            Assert.AreEqual(0, successCount);
            Assert.AreEqual(0, failureCount);

            m_DownloadModule.StopDownloading(taskId);
            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));

            Assert.AreEqual(0, successCount);
            Assert.AreEqual(1, failureCount);
        }

        [Test]
        public void TestSimpleFailingDownload()
        {
            m_DownloadTaskPool.Acquire().Returns(anyCallInfo => { return new MockFailureDownloadTask(1f); });

            int successCount = 0;
            int failureCount = 0;

            int taskId = m_DownloadModule.StartDownloading(new DownloadTaskInfo("url", "savePath", new DownloadCallbackSet
            {
                OnSuccess = (theTaskId, taskInfo) => { ++successCount; },
                OnFailure = (theTaskId, taskInfo, errorCode, errorMessage) => { ++failureCount; }
            }));

            Assert.AreEqual(0, successCount);
            Assert.AreEqual(0, failureCount);

            m_DownloadModule.Update(new TimeStruct(0f, 0f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(1.1f, 1.1f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));

            Assert.AreEqual(0, successCount);
            Assert.AreEqual(1, failureCount);
        }

        [Test]
        public void TestSeveralDownloadTasksInARow()
        {
            m_DownloadTaskPool.Acquire().Returns(anyCallInfo => { return new MockSuccessfulDownloadTask(1f); });

            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < m_DownloadModule.ConcurrentDownloadCountLimit + 1; i++)
            {
                m_DownloadModule.StartDownloading(new DownloadTaskInfo("url", "savePath", new DownloadCallbackSet
                {
                    OnSuccess = (theTaskId, taskInfo) => { ++successCount; },
                    OnFailure = (theTaskId, taskInfo, errorCode, errorMessage) => { ++failureCount; }
                }));
            }

            m_DownloadModule.Update(new TimeStruct(0f, 0f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(1.1f, 1.1f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));

            Assert.AreEqual(m_DownloadModule.ConcurrentDownloadCountLimit, successCount);
            Assert.AreEqual(0, failureCount);

            m_DownloadModule.Update(new TimeStruct(1f, 1f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));

            Assert.AreEqual(m_DownloadModule.ConcurrentDownloadCountLimit + 1, successCount);
            Assert.AreEqual(0, failureCount);
        }

        [Test]
        public void TestStopWaitingTask()
        {
            m_DownloadTaskPool.Acquire().Returns(anyCallInfo => { return new MockSuccessfulDownloadTask(1f); });

            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < m_DownloadModule.ConcurrentDownloadCountLimit + 1; i++)
            {
                m_DownloadModule.StartDownloading(new DownloadTaskInfo("url", "savePath", new DownloadCallbackSet()
                {
                    OnSuccess = (theTaskId, taskInfo) => { ++successCount; },
                    OnFailure = (theTaskId, taskInfo, errorCode, errorMessage) =>
                    {
                        ++failureCount;
                        Assert.AreEqual(DownloadErrorCode.StoppedByUser, errorCode);
                    }
                }));
            }

            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));

            m_DownloadModule.StopDownloading(m_DownloadModule.ConcurrentDownloadCountLimit + 1);

            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));

            Assert.AreEqual(1, failureCount);
            Assert.AreEqual(0, successCount);
        }

        [Test]
        public void TestTimeout()
        {
            m_DownloadTaskPool.Acquire().Returns(anyCallInfo => { return new MockSuccessfulDownloadTask(m_DownloadModule.Timeout * 2); });

            int successCount = 0;
            int failureCount = 0;

            m_DownloadModule.StartDownloading(new DownloadTaskInfo("url", "savePath", new DownloadCallbackSet
            {
                OnSuccess = (theTaskId, taskInfo) => { ++successCount; },
                OnFailure = (theTaskId, taskInfo, errorCode, errorMessage) =>
                {
                    ++failureCount;
                    Assert.AreEqual(DownloadErrorCode.Timeout, errorCode);
                }
            }));

            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(m_DownloadModule.Timeout, m_DownloadModule.Timeout, 0f, 0f));
            m_DownloadModule.Update(new TimeStruct(.1f, .1f, 0f, 0f));

            Assert.AreEqual(1, failureCount);
            Assert.AreEqual(0, successCount);
        }

        [Test]
        public void TestDownloadProgress()
        {
            m_DownloadTaskPool.Acquire().Returns(anyCallInfo => { return new MockSuccessfulDownloadTask(10.1f); });

            int onProgressCount = 0;

            m_DownloadModule.StartDownloading(new DownloadTaskInfo("url", "savePath", -1, null, new DownloadCallbackSet
            {
                OnProgress = (taskId, info, downloadedBytes) =>
                {
                    onProgressCount++;
                    Assert.AreEqual(onProgressCount, downloadedBytes);
                }
            }, null));

            var deltaTime = .01f;
            for (float time = 0f; time < 10.1f; time += deltaTime)
            {
                m_DownloadModule.Update(new TimeStruct(deltaTime, deltaTime, time, time));
            }

            Assert.AreEqual(10, onProgressCount);
        }

        [Test]
        public void TestDownloadTaskInfoCreateError()
        {
            Assert.Throws<ArgumentException>(() => new DownloadTaskInfo("url", null, size: -1L,
                crc32: null,
                callbackSet: new DownloadCallbackSet(),
                context: null));

            Assert.Throws<ArgumentException>(() => new DownloadTaskInfo(null, "savePath", size: -1L,
                crc32: null,
                callbackSet: new DownloadCallbackSet(),
                context: null));
        }

        [SetUp]
        public void SetUp()
        {
            m_ObjectPoolService = new RefPoolService();
            m_ObjectPoolService.OnInit();

            m_DownloadModule = new DownloadModule();
            m_DownloadModule.ChunkSizeToSave = 1024;
            m_DownloadModule.ConcurrentDownloadCountLimit = 2;
            m_DownloadModule.TempFileExtension = ".tmp";
            m_DownloadModule.Timeout = 10000f;
            m_DownloadTaskPool = Substitute.For<IDownloadTaskPool>();
            m_DownloadModule.DownloadTaskPool = m_DownloadTaskPool;
            m_DownloadModule.RefPoolService = m_ObjectPoolService;
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
        }
    }
}
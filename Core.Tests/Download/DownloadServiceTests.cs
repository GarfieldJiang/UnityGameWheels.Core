using NSubstitute;
using NUnit.Framework;
using System;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class DownloadServiceTests
    {
        private ITickService m_TickService = null;
        private IDownloadService m_DownloadService = null;
        private IRefPoolService m_RefPoolService = null;

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
            m_TickService = new MockTickService();
            m_RefPoolService = new RefPoolService();
            var refPoolServiceConfigReader = Substitute.For<IRefPoolServiceConfigReader>();
            refPoolServiceConfigReader.DefaultCapacity.Returns(1);
            m_RefPoolService.ConfigReader = refPoolServiceConfigReader;
            m_RefPoolService.OnInit();
            m_DownloadService = new DownloadService { TickService = m_TickService, TickOrder = 0 };
            var configReader = Substitute.For<IDownloadServiceConfigReader>();
            configReader.TempFileExtension.Returns(".tmp");
            configReader.Timeout.Returns(10000f);
            configReader.ChunkSizeToSave.Returns(1024);
            configReader.ConcurrentDownloadCountLimit.Returns(2);
            m_DownloadService.ConfigReader = configReader;
            m_DownloadService.RefPoolService = m_RefPoolService;
            m_DownloadService.OnInit();
        }

        [TearDown]
        public void TearDown()
        {
            m_DownloadService.OnShutdown();
            m_DownloadService = null;
            m_RefPoolService.OnShutdown();
            m_RefPoolService = null;
        }
    }
}
using NSubstitute;
using NUnit.Framework;
using System;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class DownloadServiceTests
    {
        private DownloadService m_DownloadService = null;
        private ITickService m_TickService = null;
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
            m_RefPoolService = new MockRefPoolService();

            var configReader = Substitute.For<IDownloadServiceConfigReader>();
            configReader.TempFileExtension.Returns(".tmp");
            configReader.Timeout.Returns(10000f);
            configReader.ChunkSizeToSave.Returns(1024);
            configReader.ConcurrentDownloadCountLimit.Returns(2);
            m_DownloadService = new DownloadService(configReader, m_RefPoolService, m_TickService, Substitute.For<ISimpleFactory<IDownloadTaskImpl>>());

            m_DownloadService.StartTicking();
        }

        [TearDown]
        public void TearDown()
        {
            m_DownloadService.Dispose();
            m_DownloadService = null;
            m_RefPoolService = null;
        }
    }
}
using NUnit.Framework;
using Moq;
using System;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class RefPoolModuleTests
    {
        private IRefPoolService m_RefPoolService = null;
        private const int DefaultCapacity = 4;

        private class PoolableObject
        {
            // Empty.
        }

        [RequireThreadSafeRefPool]
        private class PoolableObjectThreadSafety
        {
            // Empty.
        }

        [Test]
        public void TestAddAndGetRefPool()
        {
            var normalPool = m_RefPoolService.Add<PoolableObject>(DefaultCapacity);
            normalPool.ApplyCapacity();
            Assert.IsTrue(normalPool is RefPool<PoolableObject>);
            Assert.IsTrue(m_RefPoolService.Contains<PoolableObject>());
            Assert.AreEqual(normalPool, m_RefPoolService.GetOrAdd<PoolableObject>());

            var threadSafePool = m_RefPoolService.Add<PoolableObjectThreadSafety>(DefaultCapacity);
            threadSafePool.ApplyCapacity();
            Assert.IsTrue(threadSafePool is ThreadSafeRefPool<PoolableObjectThreadSafety>);
            Assert.AreEqual(threadSafePool, m_RefPoolService.GetOrAdd<PoolableObjectThreadSafety>());
            Assert.AreEqual(threadSafePool, m_RefPoolService.GetOrAdd(typeof(PoolableObjectThreadSafety)));

            var normalPool2 = m_RefPoolService.GetOrAdd<PoolableObject>();
            var normalPool3 = m_RefPoolService.GetOrAdd(typeof(PoolableObject)) as IRefPool<PoolableObject>;
            Assert.AreSame(normalPool, normalPool2);
            Assert.AreSame(normalPool, normalPool3);

            var threadSafePool2 = m_RefPoolService.GetOrAdd<PoolableObjectThreadSafety>();
            var threadSafePool3 = m_RefPoolService.GetOrAdd(typeof(PoolableObjectThreadSafety));
            Assert.AreSame(threadSafePool, threadSafePool2);
            Assert.AreSame(threadSafePool, threadSafePool3);
        }

        [Test]
        public void TestAddSameNameTwice()
        {
            m_RefPoolService.Add<PoolableObject>(DefaultCapacity);
            Assert.Throws<ArgumentException>(() => m_RefPoolService.Add<PoolableObject>(DefaultCapacity));
        }

        [Test]
        public void TestGetOrAddPool()
        {
            Assert.IsNotNull(m_RefPoolService.GetOrAdd<PoolableObject>());
        }

        [Test]
        public void TestAutoFill()
        {
            var autoFillPool = m_RefPoolService.Add<PoolableObject>(DefaultCapacity);
            autoFillPool.ApplyCapacity();
            Assert.AreEqual(DefaultCapacity, autoFillPool.Count);
        }

        [Test]
        public void TestApplyCapacity()
        {
            var pool = m_RefPoolService.Add<PoolableObject>(DefaultCapacity * 2);
            Assert.AreEqual(0, pool.Count);
            Assert.AreEqual(0, pool.Statistics.CreateCount);
            pool.ApplyCapacity();
            Assert.AreEqual(DefaultCapacity * 2, pool.Statistics.CreateCount);
            Assert.AreEqual(DefaultCapacity * 2, pool.Count);
            pool.Capacity = DefaultCapacity;
            Assert.AreEqual(DefaultCapacity * 2, pool.Count);
            Assert.AreEqual(0, pool.Statistics.DropCount);
            pool.ApplyCapacity();
            Assert.AreEqual(DefaultCapacity, pool.Count);
            Assert.AreEqual(DefaultCapacity, pool.Statistics.DropCount);

            Assert.AreEqual(0, pool.Statistics.AcquireCount);
            Assert.AreEqual(0, pool.Statistics.ReleaseCount);
        }

        [Test]
        public void TestClearAll()
        {
            m_RefPoolService.Add<PoolableObject>(DefaultCapacity).ApplyCapacity();
            m_RefPoolService.Add<PoolableObjectThreadSafety>(DefaultCapacity).ApplyCapacity();
            Assert.AreEqual(DefaultCapacity, m_RefPoolService.GetOrAdd<PoolableObject>().Count);
            Assert.AreEqual(DefaultCapacity, m_RefPoolService.GetOrAdd<PoolableObjectThreadSafety>().Count);

            m_RefPoolService.ClearAll();
            Assert.AreEqual(0, m_RefPoolService.GetOrAdd<PoolableObject>().Count);
            Assert.AreEqual(DefaultCapacity, m_RefPoolService.GetOrAdd<PoolableObject>().Statistics.DropCount);
            Assert.AreEqual(0, m_RefPoolService.GetOrAdd<PoolableObjectThreadSafety>().Count);
            Assert.AreEqual(DefaultCapacity, m_RefPoolService.GetOrAdd<PoolableObjectThreadSafety>().Statistics.DropCount);
        }

        [SetUp]
        public void SetUp()
        {
            m_RefPoolService = new RefPoolService();
            var configReader = new Mock<IRefPoolServiceConfigReader>();
            configReader.Setup(config => config.DefaultCapacity).Returns(1);
            m_RefPoolService.ConfigReader = configReader.Object;
            m_RefPoolService.OnInit();
        }

        [TearDown]
        public void TearDown()
        {
            m_RefPoolService.OnShutdown();
            m_RefPoolService = null;
        }
    }
}
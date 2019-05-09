using NUnit.Framework;
using System;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class RefPoolModuleTests
    {
        private IRefPoolModule m_RefPoolModule = null;
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
            var normalPool = m_RefPoolModule.Add<PoolableObject>(DefaultCapacity);
            normalPool.ApplyCapacity();
            Assert.IsTrue(normalPool is RefPool<PoolableObject>);
            Assert.IsTrue(m_RefPoolModule.Contains<PoolableObject>());
            Assert.AreEqual(normalPool, m_RefPoolModule.GetOrAdd<PoolableObject>());

            var threadSafePool = m_RefPoolModule.Add<PoolableObjectThreadSafety>(DefaultCapacity);
            threadSafePool.ApplyCapacity();
            Assert.IsTrue(threadSafePool is ThreadSafeRefPool<PoolableObjectThreadSafety>);
            Assert.AreEqual(threadSafePool, m_RefPoolModule.GetOrAdd<PoolableObjectThreadSafety>());
            Assert.AreEqual(threadSafePool, m_RefPoolModule.GetOrAdd(typeof(PoolableObjectThreadSafety)));

            var normalPool2 = m_RefPoolModule.GetOrAdd<PoolableObject>();
            var normalPool3 = m_RefPoolModule.GetOrAdd(typeof(PoolableObject)) as IRefPool<PoolableObject>;
            Assert.AreSame(normalPool, normalPool2);
            Assert.AreSame(normalPool, normalPool3);

            var threadSafePool2 = m_RefPoolModule.GetOrAdd<PoolableObjectThreadSafety>();
            var threadSafePool3 = m_RefPoolModule.GetOrAdd(typeof(PoolableObjectThreadSafety));
            Assert.AreSame(threadSafePool, threadSafePool2);
            Assert.AreSame(threadSafePool, threadSafePool3);
        }

        [Test]
        public void TestAddSameNameTwice()
        {
            m_RefPoolModule.Add<PoolableObject>(DefaultCapacity);
            Assert.Throws<ArgumentException>(() => m_RefPoolModule.Add<PoolableObject>(DefaultCapacity));
        }

        [Test]
        public void TestGetOrAddPool()
        {
            Assert.IsNotNull(m_RefPoolModule.GetOrAdd<PoolableObject>());
        }

        [Test]
        public void TestAutoFill()
        {
            var autoFillPool = m_RefPoolModule.Add<PoolableObject>(DefaultCapacity);
            autoFillPool.ApplyCapacity();
            Assert.AreEqual(DefaultCapacity, autoFillPool.Count);
        }

        [Test]
        public void TestApplyCapacity()
        {
            var pool = m_RefPoolModule.Add<PoolableObject>(DefaultCapacity * 2);
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
            m_RefPoolModule.Add<PoolableObject>(DefaultCapacity).ApplyCapacity();
            m_RefPoolModule.Add<PoolableObjectThreadSafety>(DefaultCapacity).ApplyCapacity();
            Assert.AreEqual(DefaultCapacity, m_RefPoolModule.GetOrAdd<PoolableObject>().Count);
            Assert.AreEqual(DefaultCapacity, m_RefPoolModule.GetOrAdd<PoolableObjectThreadSafety>().Count);

            m_RefPoolModule.ClearAll();
            Assert.AreEqual(0, m_RefPoolModule.GetOrAdd<PoolableObject>().Count);
            Assert.AreEqual(DefaultCapacity, m_RefPoolModule.GetOrAdd<PoolableObject>().Statistics.DropCount);
            Assert.AreEqual(0, m_RefPoolModule.GetOrAdd<PoolableObjectThreadSafety>().Count);
            Assert.AreEqual(DefaultCapacity, m_RefPoolModule.GetOrAdd<PoolableObjectThreadSafety>().Statistics.DropCount);
        }

        [SetUp]
        public void SetUp()
        {
            m_RefPoolModule = new RefPoolModule();
            m_RefPoolModule.Init();
        }

        [TearDown]
        public void TearDown()
        {
            m_RefPoolModule.ShutDown();
            m_RefPoolModule = null;
        }
    }
}
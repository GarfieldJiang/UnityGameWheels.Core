using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class RefPoolTests
    {
        private IRefPool<PoolableObject> m_RefPool = null;
        private const int InitCapacity = 4;

        private class PoolableObject
        {
            public static int CurrentIndex = 0;
            public int Index { get; }

            public PoolableObject()
            {
                Index = CurrentIndex++;
            }
        }

        [Test]
        public void TestAcquireAndRelease()
        {
            List<PoolableObject> objects = new List<PoolableObject>(m_RefPool.Capacity + 1);

            // Acquire Capacity + 1 objects.
            for (int i = 0; i < m_RefPool.Capacity + 1; i++)
            {
                objects.Add(m_RefPool.Acquire());
                Assert.AreEqual(i + 1, PoolableObject.CurrentIndex);
                Assert.AreEqual(i + 1, m_RefPool.Statistics.AcquireCount);
                Assert.AreEqual(i + 1, m_RefPool.Statistics.CreateCount);
            }

            // Release all of them.
            for (int i = m_RefPool.Capacity; i >= 0; i--)
            {
                var obj = objects[i];
                m_RefPool.Release(obj);
            }

            Assert.AreEqual(m_RefPool.Capacity + 1, m_RefPool.Statistics.ReleaseCount);
            Assert.AreEqual(1, m_RefPool.Statistics.DropCount);
            Assert.AreEqual(m_RefPool.Count, m_RefPool.Capacity);

            objects.Clear();

            // Acquire again. There should be only one new object.
            for (int i = 0; i < m_RefPool.Capacity + 1; i++)
            {
                m_RefPool.Acquire();
                if (i < m_RefPool.Capacity)
                {
                    Assert.AreEqual(m_RefPool.Capacity + 1, PoolableObject.CurrentIndex);
                    Assert.AreEqual(m_RefPool.Capacity + 1, m_RefPool.Statistics.CreateCount);
                }
                else
                {
                    Assert.AreEqual(m_RefPool.Capacity + 2, PoolableObject.CurrentIndex);
                    Assert.AreEqual(m_RefPool.Capacity + 2, m_RefPool.Statistics.CreateCount);
                }
            }
        }

        [Test]
        public void TestIncreaseCapacity()
        {
            var objects = new List<PoolableObject>();
            for (int i = 0; i < 2 * InitCapacity; i++)
            {
                objects.Add(m_RefPool.Acquire());
            }

            m_RefPool.Capacity *= 2;

            for (int i = 0; i < 2 * InitCapacity; i++)
            {
                m_RefPool.Release(objects[i]);
            }

            objects.Clear();

            for (int i = 0; i < 2 * InitCapacity + 1; i++)
            {
                m_RefPool.Acquire();

                if (i < 2 * InitCapacity)
                {
                    Assert.AreEqual(2 * InitCapacity, PoolableObject.CurrentIndex);
                }
                else
                {
                    Assert.AreEqual(2 * InitCapacity + 1, PoolableObject.CurrentIndex);
                }
            }
        }

        [Test]
        public void TestDecreaseCapacity()
        {
            var objects = new List<PoolableObject>();
            for (int i = 0; i < InitCapacity; i++)
            {
                objects.Add(m_RefPool.Acquire());
            }

            for (int i = 0; i < InitCapacity; i++)
            {
                m_RefPool.Release(objects[i]);
            }

            objects.Clear();

            // Decrease the capacity, but the pooled things won't be swiped out.
            int newCapacity = m_RefPool.Capacity = InitCapacity - 2;

            for (int i = 0; i < InitCapacity; i++)
            {
                objects.Add(m_RefPool.Acquire());
            }

            Assert.AreEqual(InitCapacity, PoolableObject.CurrentIndex);
            for (int i = 0; i < InitCapacity; i++)
            {
                m_RefPool.Release(objects[i]);
            }

            for (int i = 0; i < InitCapacity; i++)
            {
                m_RefPool.Acquire();
                if (i < newCapacity)
                {
                    Assert.AreEqual(InitCapacity, PoolableObject.CurrentIndex);
                }
                else
                {
                    Assert.AreEqual(InitCapacity + i - newCapacity + 1, PoolableObject.CurrentIndex);
                }
            }
        }

        [Test]
        public void TestIllegalCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => m_RefPool.Capacity = -1024);
        }

        [Test]
        public void TestClear()
        {
            var objects = new List<PoolableObject>();
            for (int i = 0; i < InitCapacity; i++)
            {
                objects.Add(m_RefPool.Acquire());
            }

            for (int i = 0; i < InitCapacity; i++)
            {
                m_RefPool.Release(objects[i]);
            }

            objects.Clear();
            Assert.AreEqual(InitCapacity, PoolableObject.CurrentIndex);

            m_RefPool.Clear();
            Assert.AreEqual(InitCapacity, m_RefPool.Statistics.DropCount);
            m_RefPool.Acquire();
            Assert.AreEqual(InitCapacity + 1, PoolableObject.CurrentIndex);
        }

        [Test]
        public void TestAutoFill()
        {
            var autoFillRefPool = (RefPool<PoolableObject>)Activator.CreateInstance(typeof(RefPool<PoolableObject>),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, new object[] {InitCapacity}, CultureInfo.InvariantCulture);
            autoFillRefPool.ApplyCapacity();
            Assert.AreEqual(InitCapacity, PoolableObject.CurrentIndex);

            for (int i = 0; i < InitCapacity; i++)
            {
                autoFillRefPool.Acquire();
                Assert.AreEqual(InitCapacity, PoolableObject.CurrentIndex);
            }
        }

        [Test]
        public void TestValidate()
        {
            Assert.AreEqual(true, m_RefPool.CheckValid());
            m_RefPool.Release(new PoolableObject());
            m_RefPool.Release(new PoolableObject());
            Assert.AreEqual(0, m_RefPool.Statistics.DropCount);
            Assert.AreEqual(true, m_RefPool.CheckValid());
            var objectToReleaseTwice = new PoolableObject();
            m_RefPool.Release(objectToReleaseTwice);
            m_RefPool.Release(objectToReleaseTwice);
            Assert.AreEqual(false, m_RefPool.CheckValid());
        }

        [SetUp]
        public void SetUp()
        {
            m_RefPool = (RefPool<PoolableObject>)Activator.CreateInstance(typeof(RefPool<PoolableObject>),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, new object[] {InitCapacity}, CultureInfo.InvariantCulture);
            PoolableObject.CurrentIndex = 0;
        }

        [TearDown]
        public void TearDown()
        {
            m_RefPool.Clear();
            m_RefPool = null;
        }
    }
}
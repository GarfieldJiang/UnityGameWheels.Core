using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class PriorityQueueTests
    {
        [Test]
        public void TestInitWithNegativeCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { new MinPriorityQueue<int, int>(-1); });
        }

        [Test]
        public void TestSimple()
        {
            var values = new List<int> {3, 5, 2, 4, 1, 7, 8, 6};
            var q = new MinPriorityQueue<int, int>();
            Assert.AreEqual(0, q.Count);
            foreach (var v in values)
            {
                q.Insert(v, v, v);
            }

            Assert.AreEqual(values.Count, q.Count);
            AssertMinHeap(q);
            AssertIndices(q);

            int min = 0;
            while (q.Count > 0)
            {
                min++;
                Assert.AreEqual(min, q.Min.Key);
                Assert.AreEqual(min, q.Min.Value);
                Assert.AreEqual(min, q.Min.Priority);
                q.PopMin();
            }

            Assert.Throws<InvalidOperationException>(() => { q.PopMin(); });
        }

        [Test]
        public void TestInsertAndDelete()
        {
            var values = new List<int>
                {31, 30, 28, 29, 26, 19, 27, 21, 20, 10, 25, 18, 11, 24, 15, 17, 6, 12, 13, 1, 7, 16, 9, 3, 2, 4, 8, 23, 22, 5, 14};
            var q = new MinPriorityQueue<int, int>();
            int count = 0;
            foreach (var v in values)
            {
                q.Insert(v + 100, v + 200, v);
                count++;
                Assert.AreEqual(count, q.Count);
                AssertMinHeap(q);
                AssertIndices(q);
            }

            Assert.False(q.Remove(19));
            Assert.True(q.Any(node => node.Key == 119));
            Assert.True(q.Any(node => node.Value == 219));
            Assert.True(q.Any(node => node.Priority == 19));
            Assert.True(q.Remove(119));
            AssertMinHeap(q);
            AssertIndices(q);
            Assert.False(q.Any(node => node.Key == 119));
            Assert.False(q.Any(node => node.Value == 219));
            Assert.False(q.Any(node => node.Priority == 19));
            Assert.False(q.Remove(119));
        }

        private void AssertMinHeap(MinPriorityQueue<int, int> MinPriorityQueue)
        {
            Assert.Multiple(() =>
            {
                var list = new List<MinPriorityQueueNode<int, int>>(MinPriorityQueue);
                int heapSize = list.Count;
                for (int i = heapSize - 1; i > 0; i--)
                {
                    Assert.GreaterOrEqual(list[i].Priority, list[(i - 1) / 2].Priority);
                }
            });
        }

        private void AssertIndices(MinPriorityQueue<int, int> MinPriorityQueue)
        {
            Assert.Multiple(() =>
            {
                int index = 0;
                foreach (var node in MinPriorityQueue)
                {
                    Assert.AreEqual(node.QueueIndex, index);
                    index++;
                }
            });
        }
    }
}
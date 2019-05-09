using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace COL.UnityGameWheels.Core
{
    public class MinPriorityQueue<TKey, TValue> : IEnumerable<MinPriorityQueueNode<TKey, TValue>>
    {
        private readonly Dictionary<TKey, MinPriorityQueueNode<TKey, TValue>> m_Dict;
        private readonly List<MinPriorityQueueNode<TKey, TValue>> m_MinBinaryHeap;
        private readonly IMinPriorityQueueNodePool<TKey, TValue> m_NodePool;

        public MinPriorityQueue() : this(0)
        {
            // Empty.
        }

        public MinPriorityQueue(IMinPriorityQueueNodePool<TKey, TValue> nodePool) : this(0, nodePool)
        {
            // Empty.
        }

        public MinPriorityQueue(int capacity) : this(capacity, null)
        {
            // Empty.
        }

        public MinPriorityQueue(int capacity, IMinPriorityQueueNodePool<TKey, TValue> nodePool)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (nodePool == null)
            {
                nodePool = new DefaultMinPriorityQueueNodePool<TKey, TValue>();
            }

            m_NodePool = nodePool;
            m_Dict = new Dictionary<TKey, MinPriorityQueueNode<TKey, TValue>>(capacity);
            m_MinBinaryHeap = new List<MinPriorityQueueNode<TKey, TValue>>(capacity);
        }

        public void Insert(TKey key, TValue value, int priority)
        {
            if (m_Dict.ContainsKey(key))
            {
                throw new InvalidOperationException($"Duplicate key '{key}'");
            }

            var node = m_NodePool.Acquire();
            if (node.Queue != null)
            {
                throw new InvalidOperationException("Queue node in use.");
            }

            node.Queue = this;
            node.Priority = priority;
            node.Key = key;
            node.Value = value;
            m_Dict.Add(key, node);
            m_MinBinaryHeap.Add(node);
            node.QueueIndex = m_MinBinaryHeap.Count - 1;
            for (int i = m_MinBinaryHeap.Count - 1; i > 0; /* Empty */)
            {
                int parent = (i - 1) / 2;
                if (m_MinBinaryHeap[i].Priority >= m_MinBinaryHeap[parent].Priority)
                {
                    break;
                }

                Swap(i, parent);
                i = parent;
            }
        }

        public bool Remove(TKey key)
        {
            if (!m_Dict.TryGetValue(key, out MinPriorityQueueNode<TKey, TValue> queueNode))
            {
                return false;
            }

            m_Dict.Remove(key);
            int index = queueNode.QueueIndex;
            if (index == m_MinBinaryHeap.Count - 1)
            {
                m_MinBinaryHeap.RemoveAt(m_MinBinaryHeap.Count - 1);
            }
            else
            {
                m_MinBinaryHeap[index] = m_MinBinaryHeap[m_MinBinaryHeap.Count - 1];
                m_MinBinaryHeap[index].QueueIndex = index;
                m_MinBinaryHeap.RemoveAt(m_MinBinaryHeap.Count - 1);
                bool up = false;
                while (index > 0 && m_MinBinaryHeap[index].Priority < m_MinBinaryHeap[(index - 1) / 2].Priority)
                {
                    up = true;
                    var parent = (index - 1) / 2;
                    Swap(index, parent);
                    index = parent;
                }

                if (!up)
                {
                    MaxHeapify(index);
                }
            }

            queueNode.Key = default(TKey);
            queueNode.Value = default(TValue);
            queueNode.Queue = null;
            queueNode.Priority = 0;
            m_NodePool.Release(queueNode);
            return true;
        }

        public IEnumerator<MinPriorityQueueNode<TKey, TValue>> GetEnumerator()
        {
            return m_MinBinaryHeap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public MinPriorityQueueNode<TKey, TValue> Min
        {
            get
            {
                if (Count <= 0)
                {
                    throw new InvalidOperationException("Empty queue.");
                }

                return m_MinBinaryHeap[0];
            }
        }

        public void PopMin()
        {
            if (Count <= 0)
            {
                throw new InvalidOperationException("Empty queue.");
            }

            Remove(m_MinBinaryHeap[0].Key);
        }

        public int Count => m_MinBinaryHeap.Count;

        public MinPriorityQueueNode<TKey, TValue> this[TKey key]
        {
            get
            {
                if (!m_Dict.TryGetValue(key, out MinPriorityQueueNode<TKey, TValue> ret))
                {
                    throw new InvalidOperationException($"Key '{key}' not found.");
                }

                return ret;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return m_Dict.ContainsKey(key);
        }

        public bool TryGetQueueNode(TKey key, out MinPriorityQueueNode<TKey, TValue> queueNode)
        {
            return m_Dict.TryGetValue(key, out queueNode);
        }

        private void MaxHeapify(int topIndex)
        {
            int largest = topIndex;
            int heapSize = m_MinBinaryHeap.Count;
            do
            {
                topIndex = largest;
                int l = topIndex * 2 + 1;
                int r = topIndex * 2 + 2;
                largest = topIndex;
                if (l < heapSize && m_MinBinaryHeap[l].Priority < m_MinBinaryHeap[largest].Priority)
                {
                    largest = l;
                }

                if (r < heapSize && m_MinBinaryHeap[r].Priority < m_MinBinaryHeap[largest].Priority)
                {
                    largest = r;
                }

                if (largest != topIndex)
                {
                    Swap(largest, topIndex);
                }
            } while (topIndex != largest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int j)
        {
            if (i == j) return;
            var tmp = m_MinBinaryHeap[i];
            m_MinBinaryHeap[i] = m_MinBinaryHeap[j];
            m_MinBinaryHeap[j] = tmp;
            m_MinBinaryHeap[i].QueueIndex = i;
            m_MinBinaryHeap[j].QueueIndex = j;
        }
    }
}
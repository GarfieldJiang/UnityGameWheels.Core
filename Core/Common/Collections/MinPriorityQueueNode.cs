namespace COL.UnityGameWheels.Core
{
    public sealed class MinPriorityQueueNode<TKey, TValue>
    {
        public int Priority { get; internal set; }

        public TKey Key { get; internal set; }

        public TValue Value { get; internal set; }

        public int QueueIndex { get; internal set; }

        internal MinPriorityQueue<TKey, TValue> Queue;
    }
}
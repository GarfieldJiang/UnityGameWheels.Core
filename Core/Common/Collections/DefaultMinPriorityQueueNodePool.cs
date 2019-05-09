namespace COL.UnityGameWheels.Core
{
    internal class DefaultMinPriorityQueueNodePool<TKey, TValue> : IMinPriorityQueueNodePool<TKey, TValue>
    {
        public MinPriorityQueueNode<TKey, TValue> Acquire()
        {
            return new MinPriorityQueueNode<TKey, TValue>();
        }

        public void Release(MinPriorityQueueNode<TKey, TValue> maxPriorityQueueNode)
        {
            // Empty.
        }
    }
}
namespace COL.UnityGameWheels.Core
{
    public interface IMinPriorityQueueNodePool<TKey, TValue>
    {
        MinPriorityQueueNode<TKey, TValue> Acquire();

        void Release(MinPriorityQueueNode<TKey, TValue> MinPriorityQueueNode);
    }
}
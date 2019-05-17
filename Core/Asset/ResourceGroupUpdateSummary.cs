namespace COL.UnityGameWheels.Core.Asset
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Resource summary for update of one resource group.
    /// </summary>
    public class ResourceGroupUpdateSummary : IDictionary<string, long>
    {
        /// <summary>
        /// How many bytes totally.
        /// </summary>
        public long TotalSize { get; internal set; }

        /// <summary>
        /// How many bytes are still to update.
        /// </summary>
        public long RemainingSize { get; internal set; }

        internal IDictionary<string, long> ResourcePathToSizeMap { get; private set; }

        public ICollection<string> Keys => ResourcePathToSizeMap.Keys;

        public ICollection<long> Values => ResourcePathToSizeMap.Values;

        public int Count => ResourcePathToSizeMap.Count;

        public bool IsReadOnly => true;

        public long this[string key]
        {
            get => ResourcePathToSizeMap[key];
            set => throw new System.NotSupportedException();
        }

        public ResourceGroupUpdateSummary()
        {
            ResourcePathToSizeMap = new Dictionary<string, long>();
        }

        public bool ContainsKey(string key)
        {
            return ResourcePathToSizeMap.ContainsKey(key);
        }

        public void Add(string key, long value)
        {
            throw new System.NotSupportedException();
        }

        public bool Remove(string key)
        {
            throw new System.NotSupportedException();
        }

        public bool TryGetValue(string key, out long value)
        {
            return ResourcePathToSizeMap.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, long> item)
        {
            throw new System.NotSupportedException();
        }

        public void Clear()
        {
            throw new System.NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, long> item)
        {
            return ResourcePathToSizeMap.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, long>[] array, int arrayIndex)
        {
            throw new System.NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, long> item)
        {
            throw new System.NotSupportedException();
        }

        public IEnumerator<KeyValuePair<string, long>> GetEnumerator()
        {
            return ResourcePathToSizeMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
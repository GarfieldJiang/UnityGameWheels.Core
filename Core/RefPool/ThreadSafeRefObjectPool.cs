using System;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Thread safe version of reference pool.
    /// </summary>
    /// <typeparam name="TObject">Object type.</typeparam>
    public class ThreadSafeRefPool<TObject> : RefPool<TObject>
        where TObject : class, new()
    {
        private readonly object m_LockObject = new object();

        internal ThreadSafeRefPool(int initialCapacity)
            : base(initialCapacity)
        {
            // Empty.
        }

        public override int Capacity
        {
            get
            {
                lock (m_LockObject)
                {
                    return base.Capacity;
                }
            }
            set
            {
                lock (m_LockObject)
                {
                    base.Capacity = value;
                }
            }
        }

        public override int Count
        {
            get
            {
                lock (m_LockObject)
                {
                    return base.Count;
                }
            }
        }

        public override RefPoolStatistics Statistics
        {
            get
            {
                lock (m_LockObject)
                {
                    return base.Statistics;
                }
            }
        }

        public override TObject Acquire()
        {
            lock (m_LockObject)
            {
                return base.Acquire();
            }
        }

        public override void Clear()
        {
            lock (m_LockObject)
            {
                base.Clear();
            }
        }

        public override void Release(TObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            lock (m_LockObject)
            {
                ReleaseInternal(obj);
            }
        }
        
        public override void ApplyCapacity()
        {
            lock (m_LockObject)
            {
                base.ApplyCapacity();
            }
        }
    }
}
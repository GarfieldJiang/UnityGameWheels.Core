using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Basic implementation of the <see cref="IRefPool{TObject}"/> interface.
    /// </summary>
    /// <typeparam name="TObject">Object type.</typeparam>
    public class RefPool<TObject> : IRefPool<TObject>
        where TObject : class, new()
    {
        private int m_Capacity = 0;

        private readonly List<TObject> m_IdleRefs = new List<TObject>();

        private readonly RefPoolStatisticsInternal m_StatisticsInternal = new RefPoolStatisticsInternal();

        internal RefPool(int initialCapacity)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Must be non-negative.");
            }

            m_Capacity = initialCapacity;
        }

        /// <inheritdoc />
        public virtual int Count => m_IdleRefs.Count;

        /// <inheritdoc />
        public virtual int Capacity
        {
            get => m_Capacity;

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be non-negative.");
                }

                m_Capacity = value;
            }
        }

        /// <inheritdoc />
        public Type ObjectType => typeof(TObject);

        /// <inheritdoc />
        public virtual RefPoolStatistics Statistics => RefPoolStatistics.FromInternal(m_StatisticsInternal);

        /// <inheritdoc />
        public virtual TObject Acquire()
        {
            m_StatisticsInternal.AcquireCount++;
            if (m_IdleRefs.Count <= 0)
            {
                m_StatisticsInternal.CreateCount++;
                return new TObject();
            }

            int last = m_IdleRefs.Count - 1;
            var ret = m_IdleRefs[last];
            m_IdleRefs.RemoveAt(last);
            return ret;
        }

        /// <inheritdoc />
        public object AcquireObject()
        {
            return Acquire();
        }

        /// <inheritdoc />
        public void ReleaseObject(object obj)
        {
            Release((TObject)obj);
        }

        /// <inheritdoc />
        public virtual void Release(TObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            ReleaseInternal(obj);
        }

        /// <inheritdoc />
        public virtual void Clear()
        {
            if (m_IdleRefs.Count > 0)
            {
                m_StatisticsInternal.DropCount += m_IdleRefs.Count;
            }

            m_IdleRefs.Clear();
        }

        /// <inheritdoc />
        public bool CheckValid()
        {
            var references = new List<TObject>(m_IdleRefs);

            for (int i = 0; i < references.Count - 1; i++)
            {
                for (int j = i + 1; j < references.Count; j++)
                {
                    if (ReferenceEquals(references[i], references[j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <inheritdoc />
        public virtual void ApplyCapacity()
        {
            while (Count < Capacity)
            {
                m_StatisticsInternal.CreateCount++;
                m_IdleRefs.Add(new TObject());
            }

            if (Count > Capacity)
            {
                int dropCount = Count - Capacity;
                m_IdleRefs.RemoveRange(Capacity, dropCount);
                m_StatisticsInternal.DropCount += dropCount;
            }
        }

        protected internal void ReleaseInternal(TObject obj)
        {
            m_StatisticsInternal.ReleaseCount++;

            if (m_IdleRefs.Count >= Capacity)
            {
                m_StatisticsInternal.DropCount++;
                return;
            }

            m_IdleRefs.Add(obj);
        }
    }
}
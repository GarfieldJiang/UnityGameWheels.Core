using System;
using System.Collections;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Default implementation of <see cref="IRefPoolModule"/> interface.
    /// </summary>
    public class RefPoolModule : BaseModule, IRefPoolModule
    {
        private Dictionary<Type, IBaseRefPool> m_RefPools = null;

        private int m_DefaultCapacity = 1;

        /// <inheritdoc />
        public int DefaultCapacity
        {
            get => m_DefaultCapacity;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be non-negative.");
                }

                m_DefaultCapacity = value;
            }
        }

        /// <inheritdoc />
        public int PoolCount => m_RefPools.Count;

        /// <inheritdoc />
        public void ClearAll()
        {
            CheckStateOrThrow();

            foreach (var kv in m_RefPools)
            {
                kv.Value.Clear();
            }
        }

        /// <inheritdoc />
        public bool Contains<T>() where T : class, new()
        {
            CheckStateOrThrow();
            return m_RefPools.ContainsKey(typeof(T));
        }

        /// <inheritdoc />
        public bool Contains(Type objectType)
        {
            CheckStateOrThrow();
            CheckTypeOrThrow(objectType);
            return m_RefPools.ContainsKey(objectType);
        }

        /// <inheritdoc />
        public IRefPool<T> Add<T>(int initCapacity) where T : class, new()
        {
            CheckStateOrThrow();

            if (initCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initCapacity), "Must be non-negative.");
            }

            var type = typeof(T);
            if (m_RefPools.ContainsKey(type))
            {
                throw new ArgumentException($"There already exists a reference pool with type '{type.FullName}'");
            }

            return DoAdd<T>(initCapacity);
        }

        /// <inheritdoc />
        public IRefPool<T> Add<T>() where T : class, new()
        {
            return Add<T>(DefaultCapacity);
        }

        /// <inheritdoc />
        public IBaseRefPool Add(Type objectType)
        {
            return Add(objectType, DefaultCapacity);
        }

        /// <inheritdoc />
        public IBaseRefPool Add(Type objectType, int initCapacity)
        {
            CheckStateOrThrow();
            CheckTypeOrThrow(objectType);

            if (initCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initCapacity), "Must be non-negative.");
            }

            if (m_RefPools.ContainsKey(objectType))
            {
                throw new ArgumentException($"There already exists a reference pool with type '{objectType.FullName}'");
            }

            return DoAdd(objectType, initCapacity);
        }

        private static void CheckTypeOrThrow(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsClass)
            {
                throw new ArgumentException("Should be a class.", nameof(type));
            }

            if (type.IsAbstract)
            {
                throw new ArgumentException("Should not be abstract.", nameof(type));
            }

            if (type.IsGenericType)
            {
                throw new ArgumentException("Should not be generic.", nameof(type));
            }

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException("Should have default constructor.", nameof(type));
            }
        }

        /// <inheritdoc />
        public IRefPool<T> GetOrAdd<T>() where T : class, new()
        {
            CheckStateOrThrow();

            if (m_RefPools.TryGetValue(typeof(T), out var baseRefPool))
            {
                return (IRefPool<T>)baseRefPool;
            }

            return DoAdd<T>(DefaultCapacity);
        }

        /// <inheritdoc />
        public IBaseRefPool GetOrAdd(Type objectType)
        {
            CheckStateOrThrow();
            CheckTypeOrThrow(objectType);

            if (m_RefPools.TryGetValue(objectType, out var ret))
            {
                return ret;
            }

            return DoAdd(objectType, DefaultCapacity);
        }

        private IRefPool<T> DoAdd<T>(int initCapacity) where T : class, new()
        {
            IRefPool<T> ret;
            if (typeof(T).GetCustomAttributes(typeof(RequireThreadSafeRefPoolAttribute), true).Length > 0)
            {
                ret = new ThreadSafeRefPool<T>(initCapacity);
            }
            else
            {
                ret = new RefPool<T>(initCapacity);
            }

            m_RefPools.Add(typeof(T), ret);
            return ret;
        }

        private IBaseRefPool DoAdd(Type type, int initCapacity)
        {
            Type refPoolType;
            if (type.GetCustomAttributes(typeof(RequireThreadSafeRefPoolAttribute), true).Length > 0)
            {
                refPoolType = typeof(RefPool<>).MakeGenericType(type);
            }
            else
            {
                refPoolType = typeof(ThreadSafeRefPool<>).MakeGenericType(type);
            }

            var ret = Activator.CreateInstance(refPoolType, initCapacity) as IBaseRefPool;
            m_RefPools.Add(type, ret);
            return ret;
        }

        /// <inheritdoc />
        public override void Init()
        {
            base.Init();
            m_RefPools = new Dictionary<Type, IBaseRefPool>();
        }

        /// <inheritdoc />
        public override void ShutDown()
        {
            ClearAll();
            m_RefPools.Clear();
            base.ShutDown();
        }

        public IEnumerator<IBaseRefPool> GetEnumerator()
        {
            CheckStateOrThrow();
            return m_RefPools.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
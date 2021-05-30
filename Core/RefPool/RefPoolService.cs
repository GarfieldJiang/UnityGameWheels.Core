using System;
using System.Collections;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Default implementation of <see cref="IRefPoolService"/> interface.
    /// </summary>
    public class RefPoolService : IRefPoolService, IDisposable
    {
        private Dictionary<Type, IBaseRefPool> m_RefPools = null;

        private readonly int m_DefaultCapacity = 0;

        /// <inheritdoc />
        public IRefPoolServiceConfigReader ConfigReader { get; }

        /// <inheritdoc />
        public int PoolCount => m_RefPools.Count;

        public RefPoolService(IRefPoolServiceConfigReader configReader)
        {
            ConfigReader = configReader;
            m_RefPools = new Dictionary<Type, IBaseRefPool>();
            if (ConfigReader.DefaultCapacity <= 0)
            {
                throw new InvalidOperationException($"{nameof(ConfigReader.DefaultCapacity)} must be positive.");
            }

            m_DefaultCapacity = ConfigReader.DefaultCapacity;
        }

        /// <inheritdoc />
        public void ClearAll()
        {
            foreach (var kv in m_RefPools)
            {
                kv.Value.Clear();
            }
        }

        /// <inheritdoc />
        public bool Contains<T>() where T : class, new()
        {
            return m_RefPools.ContainsKey(typeof(T));
        }

        /// <inheritdoc />
        public bool Contains(Type objectType)
        {
            CheckTypeOrThrow(objectType);
            return m_RefPools.ContainsKey(objectType);
        }

        /// <inheritdoc />
        public IRefPool<T> Add<T>(int initCapacity) where T : class, new()
        {
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
            return Add<T>(m_DefaultCapacity);
        }

        /// <inheritdoc />
        public IBaseRefPool Add(Type objectType)
        {
            return Add(objectType, m_DefaultCapacity);
        }

        /// <inheritdoc />
        public IBaseRefPool Add(Type objectType, int initCapacity)
        {
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
            if (m_RefPools.TryGetValue(typeof(T), out var baseRefPool))
            {
                return (IRefPool<T>)baseRefPool;
            }

            return DoAdd<T>(m_DefaultCapacity);
        }

        /// <inheritdoc />
        public IBaseRefPool GetOrAdd(Type objectType)
        {
            CheckTypeOrThrow(objectType);

            if (m_RefPools.TryGetValue(objectType, out var ret))
            {
                return ret;
            }

            return DoAdd(objectType, m_DefaultCapacity);
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

        public IEnumerator<IBaseRefPool> GetEnumerator()
        {
            return m_RefPools.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            ClearAll();
            m_RefPools.Clear();
        }
    }
}
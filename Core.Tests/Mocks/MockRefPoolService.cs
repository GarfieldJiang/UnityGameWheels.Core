using System;
using System.Collections;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Tests
{
    /// <summary>
    /// Mock ref pool service, which always returns new pools and new instances.
    /// </summary>
    public class MockRefPoolService : IRefPoolService
    {
        private class MockRefPool<TObject> : IRefPool<TObject> where TObject : class, new()
        {
            public int Capacity { get; set; }
            public int Count { get; }
            public Type ObjectType => typeof(TObject);
            public RefPoolStatistics Statistics { get; }

            public void Clear()
            {
            }

            public void ApplyCapacity()
            {
            }

            public bool CheckValid()
            {
                return true;
            }

            public void ReleaseObject(object obj)
            {
            }

            public object AcquireObject()
            {
                return Acquire();
            }

            public TObject Acquire()
            {
                return new TObject();
            }

            public void Release(TObject obj)
            {
            }
        }

        public IEnumerator<IBaseRefPool> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IRefPoolServiceConfigReader ConfigReader { get; }
        public int PoolCount { get; }

        public IRefPool<TObject> Add<TObject>(int initCapacity) where TObject : class, new()
        {
            return Add<TObject>();
        }

        public IRefPool<TObject> Add<TObject>() where TObject : class, new()
        {
            return new MockRefPool<TObject>();
        }

        public IBaseRefPool Add(Type objectType, int initCapacity)
        {
            throw new NotImplementedException();
        }

        public IBaseRefPool Add(Type objectType)
        {
            throw new NotImplementedException();
        }

        public IRefPool<TObject> GetOrAdd<TObject>() where TObject : class, new()
        {
            return new MockRefPool<TObject>();
        }

        public IBaseRefPool GetOrAdd(Type objectType)
        {
            throw new NotImplementedException();
        }

        public bool Contains<TObject>() where TObject : class, new()
        {
            return true;
        }

        public bool Contains(Type objectType)
        {
            return true;
        }

        public void ClearAll()
        {
        }
    }
}
using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Reference pool module interface.
    /// </summary>
    public interface IRefPoolModule : IModule, IEnumerable<IBaseRefPool>
    {
        /// <summary>
        /// Default capacity of newly added pools.
        /// </summary>
        int DefaultCapacity { get; set; }

        /// <summary>
        /// How many pools we have.
        /// </summary>
        int PoolCount { get; }

        /// <summary>
        /// Add a new reference pool.
        /// </summary>
        /// <param name="initCapacity">Initial capacity.</param>
        /// <typeparam name="TObject">Object type.</typeparam>
        /// <returns>The pool.</returns>
        IRefPool<TObject> Add<TObject>(int initCapacity) where TObject : class, new();

        /// <summary>
        /// Add a new reference pool with <see cref="DefaultCapacity"/>.
        /// </summary>
        /// <typeparam name="TObject">Initial capacity.</typeparam>
        /// <returns>The pool.</returns>
        IRefPool<TObject> Add<TObject>() where TObject : class, new();

        /// <summary>
        /// Add a new reference pool.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        /// <param name="initCapacity">Initial capacity.</param>
        /// <returns>The pool.</returns>
        IBaseRefPool Add(Type objectType, int initCapacity);

        /// <summary>
        /// Add a new reference pool with <see cref="DefaultCapacity"/>.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        /// <returns>The pool.</returns>
        IBaseRefPool Add(Type objectType);

        /// <summary>
        /// Get a reference pool. Add one if it doesn't exist.
        /// </summary>
        /// <typeparam name="TObject">Object type.</typeparam>
        /// <returns>The pool.</returns>
        IRefPool<TObject> GetOrAdd<TObject>() where TObject : class, new();

        /// <summary>
        /// Get a reference pool. Add one if it doesn't exist.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        /// <returns>The pool.</returns>
        IBaseRefPool GetOrAdd(Type objectType);

        /// <summary>
        /// Whether we have such a pool.
        /// </summary>
        /// <typeparam name="TObject">Object type.</typeparam>
        /// <returns>Whether we have such a pool.</returns>
        bool Contains<TObject>() where TObject : class, new();

        /// <summary>
        /// Whether we have such a pool.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        /// <returns>Whether we have such a pool.</returns>
        bool Contains(Type objectType);

        /// <summary>
        /// Clear all the pools we have.
        /// </summary>
        void ClearAll();
    }
}
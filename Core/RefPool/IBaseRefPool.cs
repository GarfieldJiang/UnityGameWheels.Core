using System;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Non-generic base interface for reference pool.
    /// </summary>
    public interface IBaseRefPool
    {
        /// <summary>
        /// How many objects to cache at most.
        /// </summary>
        int Capacity { get; set; }

        /// <summary>
        /// How many objects are currently cached.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Object type.
        /// </summary>
        Type ObjectType { get; }

        /// <summary>
        /// Statistics.
        /// </summary>
        RefPoolStatistics Statistics { get; }

        /// <summary>
        /// Clear all the cached objects.
        /// </summary>
        void Clear();

        /// <summary>
        /// Make <see cref="Count"/> equal to <see cref="Capacity"/>.
        /// </summary>
        void ApplyCapacity();

        /// <summary>
        /// The pool is valid if no objects is cached twice or more.
        /// </summary>
        /// <returns>Whether the pool is valid.</returns>
        bool CheckValid();

        /// <summary>
        /// Release an object back to this pool.
        /// </summary>
        /// <param name="obj">Object to release.</param>
        void ReleaseObject(object obj);

        /// <summary>
        /// Acquire an object from this pool.
        /// </summary>
        /// <returns>The object.</returns>
        object AcquireObject();
    }
}
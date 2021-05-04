using System;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface of tick service. Based on Unity3D's terms Update and LateUpdate.
    /// </summary>
    public interface ITickService
    {
        /// <summary>
        /// Add callback for engine's Update tick.
        /// </summary>
        /// <param name="updateFunc">The callback.</param>
        /// <param name="order">The order.</param>
        void AddUpdateCallback(Action<TimeStruct> updateFunc, int order);

        /// <summary>
        /// Remove callback for engine's Update tick.
        /// </summary>
        /// <param name="updateFunc">The callback.</param>
        /// <returns>Whether the callback is successfully removed.</returns>
        bool RemoveUpdateCallback(Action<TimeStruct> updateFunc);

        /// <summary>
        /// Add callback for engine's LateUpdate tick.
        /// </summary>
        /// <param name="lateUpdateFunc">The callback.</param>
        /// <param name="order">The order.</param>
        void AddLateUpdateCallback(Action<TimeStruct> lateUpdateFunc, int order);

        /// <summary>
        /// Remove callback for engine's LateUpdate tick.
        /// </summary>
        /// <param name="lateUpdateFunc">The callback.</param>
        /// <returns>Whether the callback is successfully removed.</returns>
        bool RemoveLateUpdateCallback(Action<TimeStruct> lateUpdateFunc);
    }
}
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
        /// <param name="updateCallback">The callback.</param>
        /// <param name="order">The order.</param>
        void AddUpdateCallback(Action<TimeStruct> updateCallback, int order);

        /// <summary>
        /// Remove callback for engine's Update tick.
        /// </summary>
        /// <param name="updateCallback">The callback.</param>
        /// <returns>Whether the callback is successfully removed.</returns>
        bool RemoveUpdateCallback(Action<TimeStruct> updateCallback);

        /// <summary>
        /// Add callback for engine's LateUpdate tick.
        /// </summary>
        /// <param name="lateUpdateCallback">The callback.</param>
        /// <param name="order">The order.</param>
        void AddLateUpdateCallback(Action<TimeStruct> lateUpdateCallback, int order);

        /// <summary>
        /// Remove callback for engine's LateUpdate tick.
        /// </summary>
        /// <param name="lateUpdateCallback">The callback.</param>
        /// <returns>Whether the callback is successfully removed.</returns>
        bool RemoveLateUpdateCallback(Action<TimeStruct> lateUpdateCallback);
    }
}
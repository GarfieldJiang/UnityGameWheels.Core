using System;

namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Interface for tickable IOC containers.
    /// </summary>
    public interface ITickableContainer : IContainer, ITickable
    {
        /// <summary>
        /// Whether the container is requesting a shutdown.
        /// </summary>
        bool IsRequestingShutdown { get; }

        /// <summary>
        /// Requests an asynchronous (and hence safe) procedure of shutting down the container.
        /// </summary>
        void RequestShutdown();

        /// <summary>
        /// Shutdown completion event.
        /// </summary>
        event Action OnShutdownComplete;
    }
}
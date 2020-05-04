using System;

namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Interface for binding data.
    /// </summary>
    public interface IBindingData
    {
        /// <summary>
        /// Add an alias of the current binding.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>Self.</returns>
        IBindingData Alias(string alias);

        /// <summary>
        /// The interface type.
        /// </summary>
        Type InterfaceType { get; }

        /// <summary>
        /// The implementation type.
        /// </summary>
        Type ImplType { get; }

        /// <summary>
        /// The service name.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Whether the service life cycle is managed by the container.
        /// </summary>
        bool LifeCycleManaged { get; }

        /// <summary>
        /// Add a callback before <see cref="ILifeCycle.OnInit"/> is called, if the service implementation
        /// is a <see cref="ILifeCycle"/> and <see cref="LifeCycleManaged"/> is true.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Self.</returns>
        IBindingData OnPreInit(Action<object> callback);

        /// <summary>
        /// Add a callback after <see cref="ILifeCycle.OnInit"/> is called, if the service implementation
        /// is a <see cref="ILifeCycle"/> and <see cref="LifeCycleManaged"/> is true.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Self.</returns>
        IBindingData OnPostInit(Action<object> callback);

        /// <summary>
        /// Add a callback before <see cref="ILifeCycle.OnShutdown"/> is called, if the service implementation
        /// is a <see cref="ILifeCycle"/> and <see cref="LifeCycleManaged"/> is true.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Self.</returns>
        IBindingData OnPreShutdown(Action<object> callback);

        /// <summary>
        /// Add a callback after <see cref="ILifeCycle.OnShutdown"/> is called, if the service implementation
        /// is a <see cref="ILifeCycle"/> and <see cref="LifeCycleManaged"/> is true.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Self.</returns>
        IBindingData OnPostShutdown(Action callback);
    }
}
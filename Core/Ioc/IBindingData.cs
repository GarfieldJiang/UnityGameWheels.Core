using System;

namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Interface for binding data.
    /// </summary>
    public interface IBindingData
    {

        /// <summary>
        /// The interface type.
        /// </summary>
        Type InterfaceType { get; }

        /// <summary>
        /// The implementation type.
        /// </summary>
        Type ImplType { get; }

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
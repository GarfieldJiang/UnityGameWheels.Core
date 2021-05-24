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
        
        IBindingData OnInstanceCreated(Action<object> callback);

        IBindingData OnPreDispose(Action<object> callback);

        IBindingData OnDisposed(Action callback);
    }
}
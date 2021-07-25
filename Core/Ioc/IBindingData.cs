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

        ILifeStyle LifeStyle { get; }

        IBindingData SetConstructor(params Type[] paramTypes);

        IBindingData AddPropertyInjections(params PropertyInjection[] propertyInjections);

        IBindingData OnInstanceCreated(Action<object> callback);

        IBindingData OnPreDispose(Action<object> callback);

        IBindingData OnDisposed(Action callback);
    }
}
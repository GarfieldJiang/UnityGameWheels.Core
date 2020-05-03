using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Interface for IOC containers.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Whether the container is shutting down.
        /// </summary>
        bool IsShuttingDown { get; }

        /// <summary>
        /// Whether the container is already shut.
        /// </summary>
        bool IsShut { get; }

        /// <summary>
        /// Binds a singleton service.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="implType">The implementation type.</param>
        /// <returns>The binding data.</returns>
        IBindingData BindSingleton(string serviceName, Type implType);

        /// <summary>
        /// Binds a singleton service.
        /// </summary>
        /// <param name="interfaceType">The interface type.</param>
        /// <param name="implType">The implementation type.</param>
        /// <returns>The binding data.</returns>
        IBindingData BindSingleton(Type interfaceType, Type implType);

        /// <summary>
        /// Binds an instance.
        /// </summary>
        /// <param name="interfaceType">The interface type.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>The binding data.</returns>
        IBindingData BindInstance(Type interfaceType, object instance);

        /// <summary>
        /// Binds an instance.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>The binding data.</returns>
        IBindingData BindInstance(string serviceName, object instance);

        /// <summary>
        /// Gets the binding data of a service.
        /// </summary>
        /// <param name="serviceName">The service name or alias.</param>
        /// <returns>The binding data.</returns>
        IBindingData GetBindingData(string serviceName);

        /// <summary>
        /// Gets the binding data of a service.
        /// </summary>
        /// <param name="interfaceType">The service abstraction type.</param>
        /// <returns>The binding data.</returns>
        IBindingData GetBindingData(Type interfaceType);

        /// <summary>
        /// Tries to get the binding data of a service.
        /// </summary>
        /// <param name="serviceName">The service name or alias.</param>
        /// <param name="bindingData">The binding data if any.</param>
        /// <returns>True if the binding exists.</returns>
        bool TryGetBindingData(string serviceName, out IBindingData bindingData);

        /// <summary>
        /// Tries to get the binding data of a service.
        /// </summary>
        /// <param name="interfaceType">The service type.</param>
        /// <param name="bindingData">The binding data if any.</param>
        /// <returns>True if the binding exists.</returns>
        bool TryGetBindingData(Type interfaceType, out IBindingData bindingData);


        /// <summary>
        /// Gets whether a service is bounded.
        /// </summary>
        /// <param name="serviceName">The service name or alias.</param>
        /// <returns>Whether the service is bounded.</returns>
        bool IsBound(string serviceName);

        /// <summary>
        /// Gets whether a service is bounded.
        /// </summary>
        /// <param name="interfaceType">The service type.</param>
        /// <returns>Whether the service is bounded.</returns>
        bool TypeIsBound(Type interfaceType);

        /// <summary>
        /// Makes the service.
        /// </summary>
        /// <param name="serviceName">The service name or alias.</param>
        /// <returns>The service instance.</returns>
        object Make(string serviceName);

        /// <summary>
        /// Makes the service.
        /// </summary>
        /// <param name="interfaceType">The service type.</param>
        /// <returns>The service instance.</returns>
        object Make(Type interfaceType);

        /// <summary>
        /// Shut down the container.
        /// </summary>
        void ShutDown();

        /// <summary>
        /// Adds an alias to the given service.
        /// </summary>
        /// <param name="serviceName">The service name or existing alias.</param>
        /// <param name="alias">The new alias.</param>
        void Alias(string serviceName, string alias);

        /// <summary>
        /// Checks if the given service name is an alias.
        /// </summary>
        /// <param name="serviceName">The service name or alias.</param>
        /// <returns>Whether the given service name is an alias.</returns>
        bool IsAlias(string serviceName);

        /// <summary>
        /// Convert an interface type to service name.
        /// </summary>
        /// <param name="interfaceType">The service interface type.</param>
        /// <returns>The service name</returns>
        string TypeToServiceName(Type interfaceType);

        /// <summary>
        /// Given the alias of service name of a service, turns it into the service name.
        /// </summary>
        /// <param name="serviceName">The service name or alias.</param>
        /// <returns></returns>
        string Dealias(string serviceName);

        /// <summary>
        /// Get all existing singletons.
        /// </summary>
        /// <returns>All existing singletons.</returns>
        IEnumerable<KeyValuePair<string, object>> GetSingletons();
    }
}
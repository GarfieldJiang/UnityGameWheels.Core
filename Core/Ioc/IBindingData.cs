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
    }
}
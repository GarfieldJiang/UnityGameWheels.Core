using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Default implementation of tickable containers.
    /// </summary>
    /// <remarks>NOT thread-safe.</remarks>
    public class TickableContainer : ITickableContainer
    {
        private readonly List<ITickable> m_TickableInstances;
        private readonly List<ITickable> m_TickableInstancesCopied;
        private readonly Container m_InternalContainer;
        private bool m_IsRequestingShutdown = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="estimatedServiceCount">Estimated service count.</param>
        public TickableContainer(int estimatedServiceCount = 64)
        {
            m_InternalContainer = new Container(estimatedServiceCount);
            m_TickableInstances = new List<ITickable>(estimatedServiceCount);
            m_TickableInstancesCopied = new List<ITickable>(estimatedServiceCount);
        }

        public bool IsShuttingDown => m_InternalContainer.IsShuttingDown;
        public bool IsShut => m_InternalContainer.IsShut;

        /// <inheritdoc />
        public IBindingData BindSingleton(string serviceName, Type implType)
        {
            GuardNotRequestingShutdown();
            return m_InternalContainer.BindSingleton(serviceName, implType);
        }

        /// <inheritdoc />
        public IBindingData BindSingleton(Type interfaceType, Type implType)
        {
            GuardNotRequestingShutdown();
            return m_InternalContainer.BindSingleton(interfaceType, implType);
        }

        /// <inheritdoc />
        public IBindingData GetBindingData(string serviceName)
        {
            GuardNotRequestingShutdown();
            return m_InternalContainer.GetBindingData(serviceName);
        }

        /// <inheritdoc />
        public IBindingData GetBindingData(Type interfaceType)
        {
            return m_InternalContainer.GetBindingData(interfaceType);
        }

        /// <inheritdoc />
        public bool TryGetBindingData(string serviceName, out IBindingData bindingData)
        {
            return m_InternalContainer.TryGetBindingData(serviceName, out bindingData);
        }

        /// <inheritdoc />
        public bool TryGetBindingData(Type interfaceType, out IBindingData bindingData)
        {
            return m_InternalContainer.TryGetBindingData(interfaceType, out bindingData);
        }

        /// <inheritdoc />
        public bool IsBound(string serviceName)
        {
            return m_InternalContainer.IsBound(serviceName);
        }

        /// <inheritdoc />
        public bool TypeIsBound(Type interfaceType)
        {
            return m_InternalContainer.TypeIsBound(interfaceType);
        }

        /// <inheritdoc />
        public object Make(string serviceName)
        {
            GuardNotRequestingShutdown();
            var serviceInstance = m_InternalContainer.Make(serviceName);
            if (serviceInstance is ITickable tickableService)
            {
                m_TickableInstances.Add(tickableService);
            }

            return serviceInstance;
        }

        /// <inheritdoc />
        public object Make(Type interfaceType)
        {
            GuardNotRequestingShutdown();
            var serviceInstance = m_InternalContainer.Make(interfaceType);
            if (serviceInstance is ITickable tickableService)
            {
                m_TickableInstances.Add(tickableService);
            }

            return serviceInstance;
        }

        /// <inheritdoc />
        public bool IsRequestingShutdown => m_IsRequestingShutdown;

        /// <inheritdoc />
        public void RequestShutdown()
        {
            GuardNotRequestingShutdown();
            m_InternalContainer.GuardNotShuttingDownOrShut();
            m_IsRequestingShutdown = true;
        }

        private Action m_OnShutdownComplete;

        /// <inheritdoc />
        public event Action OnShutdownComplete
        {
            add => m_OnShutdownComplete += value;
            remove => m_OnShutdownComplete -= value;
        }

        /// <inheritdoc />
        public void ShutDown()
        {
            GuardNotRequestingShutdown();
            m_TickableInstances.Clear();
            m_InternalContainer.ShutDown();
            m_OnShutdownComplete?.Invoke();
        }

        /// <inheritdoc />
        public void Alias(string serviceName, string alias)
        {
            GuardNotRequestingShutdown();
            m_InternalContainer.Alias(serviceName, alias);
        }

        /// <inheritdoc />
        public bool IsAlias(string serviceName) => m_InternalContainer.IsAlias(serviceName);

        /// <inheritdoc />
        public string TypeToServiceName(Type interfaceType) => m_InternalContainer.TypeToServiceName(interfaceType);

        /// <inheritdoc />
        public string Dealias(string serviceName)
        {
            GuardNotRequestingShutdown();
            return m_InternalContainer.Dealias(serviceName);
        }

        /// <inheritdoc />
        public void OnUpdate(TimeStruct timeStruct)
        {
            m_TickableInstancesCopied.Clear();
            m_TickableInstancesCopied.AddRange(m_TickableInstances);
            foreach (var tickableInstance in m_TickableInstancesCopied)
            {
                tickableInstance.OnUpdate(timeStruct);
            }

            m_TickableInstancesCopied.Clear();
            TackleShutdownRequest();
        }

        private void TackleShutdownRequest()
        {
            if (!m_IsRequestingShutdown)
            {
                return;
            }

            while (m_InternalContainer.ServicesToShutdown.Count > 0)
            {
                var node = m_InternalContainer.ServicesToShutdown.Min;
                if (!node.Value.CanSafelyShutDown)
                {
                    break;
                }

                node.Value.OnShutdown();
                m_InternalContainer.ServicesToShutdown.Remove(node.Key);
            }

            if (m_InternalContainer.ServicesToShutdown.Count == 0)
            {
                m_IsRequestingShutdown = false;
                m_InternalContainer.ShutDown();
                m_OnShutdownComplete?.Invoke();
            }
        }

        private void GuardNotRequestingShutdown()
        {
            Guard.RequireFalse<InvalidOperationException>(m_IsRequestingShutdown, "Container is requesting shutdown.");
        }
    }
}
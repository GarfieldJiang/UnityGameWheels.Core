using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Default implementation of containers.
    /// </summary>
    /// <remarks>NOT thread-safe. And won't consider <see cref="ILifeCycle.CanSafelyShutDown"/>.</remarks>
    public class Container : IContainer
    {
        private readonly Dictionary<string, IBindingData> m_ServiceNameToBindingDataMap;
        private readonly Dictionary<Type, IBindingData> m_InterfaceTypeToBindingDataMap;
        private readonly Dictionary<string, object> m_ServiceNameToSingletonMap;
        private readonly Dictionary<string, string> m_AliasToServiceNameMap;
        private readonly Stack<IBindingData> m_BindingDatasToBuild;
        private readonly Queue<string> m_ServicesToInit;
        internal readonly MinPriorityQueue<string, ILifeCycle> ServicesToShutdown;
        private int m_ServiceInitCounter = 0;

        private bool m_IsShuttingDown = false;
        private bool m_IsShut = false;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="estimatedServiceCount">Estimated service count.</param>
        public Container(int estimatedServiceCount = 64)
        {
            Guard.RequireTrue<ArgumentOutOfRangeException>(estimatedServiceCount > 0,
                $"Argument '{nameof(estimatedServiceCount)}' must be positive.");
            m_ServiceNameToBindingDataMap = new Dictionary<string, IBindingData>(estimatedServiceCount);
            m_InterfaceTypeToBindingDataMap = new Dictionary<Type, IBindingData>(estimatedServiceCount);
            m_ServiceNameToSingletonMap = new Dictionary<string, object>(estimatedServiceCount);
            m_BindingDatasToBuild = new Stack<IBindingData>(estimatedServiceCount);
            m_ServicesToInit = new Queue<string>(estimatedServiceCount);
            m_AliasToServiceNameMap = new Dictionary<string, string>(estimatedServiceCount);
            ServicesToShutdown = new MinPriorityQueue<string, ILifeCycle>();
        }

        /// <inheritdoc />
        public bool IsShuttingDown => m_IsShuttingDown;

        /// <inheritdoc />
        public bool IsShut => m_IsShut;

        /// <inheritdoc />
        public IBindingData BindSingleton(string serviceName, Type implType)
        {
            GuardNotShuttingDownOrShut();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            GuardUnbound(Dealias(serviceName));
            GuardImplType(implType);
            var bindingData = new BindingData(this)
            {
                ServiceName = serviceName,
                ImplType = implType,
                LifeCycleManaged = true,
            };
            m_ServiceNameToBindingDataMap[serviceName] = bindingData;
            return bindingData;
        }


        /// <inheritdoc />
        public IBindingData BindSingleton(Type interfaceType, Type implType)
        {
            GuardNotShuttingDownOrShut();
            GuardInterfaceType(interfaceType);
            GuardImplType(implType);
            if (!interfaceType.IsAssignableFrom(implType))
            {
                throw new InvalidOperationException($"{nameof(interfaceType)} is not assignable from {nameof(implType)}.");
            }

            var bindingData = (BindingData)BindSingleton(TypeToServiceName(interfaceType), implType);
            bindingData.InterfaceType = interfaceType;
            m_InterfaceTypeToBindingDataMap[interfaceType] = bindingData;
            return bindingData;
        }

        /// <inheritdoc />
        public IBindingData BindInstance(Type interfaceType, object instance)
        {
            GuardNotShuttingDownOrShut();
            GuardInterfaceType(interfaceType);
            Guard.RequireNotNull<ArgumentNullException>(instance, $"Invalid '{nameof(instance)}'.");
            var implType = instance.GetType();
            GuardImplType(implType);
            if (!interfaceType.IsAssignableFrom(implType))
            {
                throw new InvalidOperationException($"{nameof(interfaceType)} is not assignable from {nameof(implType)}.");
            }

            var bindingData = (BindingData)BindInstance(TypeToServiceName(interfaceType), instance);
            bindingData.InterfaceType = interfaceType;
            bindingData.ImplType = implType;
            m_InterfaceTypeToBindingDataMap[interfaceType] = bindingData;
            return bindingData;
        }

        /// <inheritdoc />
        public IBindingData BindInstance(string serviceName, object instance)
        {
            GuardNotShuttingDownOrShut();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            GuardUnbound(Dealias(serviceName));
            var implType = instance.GetType();
            Guard.RequireNotNull<ArgumentNullException>(instance, $"Invalid '{nameof(instance)}'.");
            GuardImplType(implType);
            var bindingData = new BindingData(this)
            {
                ServiceName = serviceName,
                ImplType = implType,
                LifeCycleManaged = false,
            };
            m_ServiceNameToBindingDataMap[serviceName] = bindingData;
            m_ServiceNameToSingletonMap[serviceName] = instance;
            return bindingData;
        }

        /// <inheritdoc />
        public IBindingData GetBindingData(string serviceName)
        {
            if (!TryGetBindingData(serviceName, out var bindingData))
            {
                throw new InvalidOperationException($"Service name '{serviceName}' is not bound yet.");
            }

            return bindingData;
        }

        /// <inheritdoc />
        public IBindingData GetBindingData(Type interfaceType)
        {
            if (!TryGetBindingData(interfaceType, out var bindingData))
            {
                throw new InvalidOperationException($"Service type '{interfaceType}' is not bound yet.");
            }

            return bindingData;
        }

        /// <inheritdoc />
        public bool TryGetBindingData(string serviceName, out IBindingData bindingData)
        {
            GuardNotShuttingDownOrShut();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            serviceName = Dealias(serviceName);
            return m_ServiceNameToBindingDataMap.TryGetValue(serviceName, out bindingData);
        }

        /// <inheritdoc />
        public bool TryGetBindingData(Type interfaceType, out IBindingData bindingData)
        {
            GuardNotShuttingDownOrShut();
            Guard.RequireNotNull<ArgumentNullException>(interfaceType, $"Invalid '{nameof(interfaceType)}'.");
            return m_InterfaceTypeToBindingDataMap.TryGetValue(interfaceType, out bindingData);
        }

        /// <inheritdoc />
        public bool IsBound(string serviceName)
        {
            GuardNotShuttingDownOrShut();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            return m_ServiceNameToBindingDataMap.ContainsKey(Dealias(serviceName));
        }

        /// <inheritdoc />
        public bool TypeIsBound(Type interfaceType)
        {
            GuardNotShuttingDownOrShut();
            GuardInterfaceType(interfaceType);
            return m_InterfaceTypeToBindingDataMap.ContainsKey(interfaceType);
        }

        /// <inheritdoc />
        public object Make(string serviceName)
        {
            GuardNotShuttingDownOrShut();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            return MakeInternal((BindingData)GetBindingData(serviceName));
        }

        /// <inheritdoc />
        public object Make(Type interfaceType)
        {
            GuardNotShuttingDownOrShut();
            return MakeInternal((BindingData)GetBindingData(interfaceType));
        }

        private object MakeInternal(BindingData bindingData)
        {
            object serviceInstance;
            try
            {
                serviceInstance = ResolveInternal(bindingData);
                InitServices();
            }
            finally
            {
                m_BindingDatasToBuild.Clear();
                m_ServicesToInit.Clear();
            }

            return serviceInstance;
        }

        private void InitServices()
        {
            while (m_ServicesToInit.Count > 0)
            {
                var serviceNameToInit = m_ServicesToInit.Dequeue();
                var serviceInstance = m_ServiceNameToSingletonMap[serviceNameToInit];
                var bindingData = m_ServiceNameToBindingDataMap[serviceNameToInit];

                if (!((BindingData)bindingData).LifeCycleManaged)
                {
                    continue;
                }

                if (!(serviceInstance is ILifeCycle lifeCycleInstance))
                {
                    continue;
                }

                m_ServiceInitCounter++;
                lifeCycleInstance.OnInit();
                ServicesToShutdown.Insert(serviceNameToInit, lifeCycleInstance, -m_ServiceInitCounter);
            }
        }

        private object ResolveInternal(BindingData bindingData)
        {
            if (m_BindingDatasToBuild.Contains(bindingData))
            {
                throw new InvalidOperationException("Cyclic dependencies are not supported.");
            }

            m_BindingDatasToBuild.Push(bindingData);
            var instanceType = bindingData.ImplType;

            if (!m_ServiceNameToSingletonMap.TryGetValue(bindingData.ServiceName, out object ret))
            {
                ret = Activator.CreateInstance(instanceType);
                foreach (var property in instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
                {
                    if (property.GetCustomAttribute<InjectAttribute>() == null)
                    {
                        continue;
                    }

                    var dependency = ResolveInternal((BindingData)GetBindingData(property.PropertyType));
                    property.SetValue(ret, dependency);
                }

                m_ServiceNameToSingletonMap[bindingData.ServiceName] = ret;
                m_ServicesToInit.Enqueue(bindingData.ServiceName);
            }

            m_BindingDatasToBuild.Pop();
            return ret;
        }

        /// <inheritdoc />
        public void ShutDown()
        {
            GuardNotShuttingDownOrShut();
            m_IsShuttingDown = true;
            while (ServicesToShutdown.Count > 0)
            {
                var node = ServicesToShutdown.Min;
                ShutDown(node.Key);
                ServicesToShutdown.PopMin();
            }

            Clear();
            m_IsShuttingDown = false;
            m_IsShut = true;
        }

        public void Alias(string serviceName, string alias)
        {
            GuardNotShuttingDownOrShut();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(alias)}'.");
            Guard.RequireFalse<InvalidOperationException>(serviceName == alias, $"'{nameof(serviceName)}' and '{nameof(alias)}' are identical.");
            GuardUnbound(alias);
            if (!TryGetBindingData(serviceName, out var bindingData))
            {
                throw new InvalidOperationException($"{nameof(serviceName)} '{serviceName}' is not bounded.");
            }

            ((BindingData)bindingData).AliasInternal(alias);
            m_AliasToServiceNameMap.Add(alias, ((BindingData)bindingData).ServiceName);
        }

        public bool IsAlias(string serviceName) => m_AliasToServiceNameMap.ContainsKey(serviceName);

        public string TypeToServiceName(Type interfaceType)
        {
            Guard.RequireNotNull<ArgumentNullException>(interfaceType, $"Invalid '{nameof(interfaceType)}.");
            return interfaceType.ToString();
        }

        internal bool ShutDown(string serviceName)
        {
            if (!m_ServiceNameToSingletonMap.TryGetValue(serviceName, out var serviceInstance))
            {
                return false;
            }

            m_ServiceNameToSingletonMap.Remove(serviceName);
            if (serviceInstance is ILifeCycle lifeCycleInstance)
            {
                lifeCycleInstance.OnShutdown();
            }

            return true;
        }

        internal void Clear()
        {
            m_BindingDatasToBuild.Clear();
            m_ServiceNameToSingletonMap.Clear();
            m_InterfaceTypeToBindingDataMap.Clear();
            m_ServiceNameToBindingDataMap.Clear();
        }

        /// <inheritdoc />
        public string Dealias(string serviceName)
        {
            GuardNotShuttingDownOrShut();
            return m_AliasToServiceNameMap.TryGetValue(serviceName, out var realServiceName) ? realServiceName : serviceName;
        }

        internal void GuardNotShuttingDownOrShut()
        {
            Guard.RequireFalse<InvalidOperationException>(m_IsShuttingDown || m_IsShut, "The container is shutting down or already shut.");
        }

        private void GuardImplType(Type implType)
        {
            Guard.RequireNotNull<ArgumentNullException>(implType, $"Invalid {nameof(implType)}.");
            Guard.RequireTrue<ArgumentException>(implType.IsClass && !implType.IsAbstract && !implType.IsInterface
                                                 && implType.GetConstructors().Any(c => c.GetParameters().Length == 0),
                $"{nameof(implType)} '{implType}' is not supported");
        }

        private void GuardInterfaceType(Type interfaceType)
        {
            Guard.RequireNotNull<ArgumentNullException>(interfaceType, $"Invalid {nameof(interfaceType)}.");
            Guard.RequireTrue<ArgumentException>(!interfaceType.IsAbstract || !interfaceType.IsSealed,
                $"{nameof(interfaceType)} '{interfaceType}' is not supported.");
        }

        private void GuardUnbound(string serviceName)
        {
            Guard.RequireFalse<InvalidOperationException>(m_ServiceNameToBindingDataMap.ContainsKey(Dealias(serviceName)),
                $"Service name or alias '{serviceName}' is already bound.");
        }
    }
}
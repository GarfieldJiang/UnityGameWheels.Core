using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Default implementation of a container.
    /// </summary>
    /// <remarks>NOT thread-safe.</remarks>
    public sealed class Container : IDisposable
    {
        private readonly Dictionary<string, IBindingData> m_ServiceNameToBindingDataMap;
        private readonly Dictionary<Type, IBindingData> m_InterfaceTypeToBindingDataMap;
        private readonly Dictionary<string, object> m_ServiceNameToSingletonMap;
        private readonly Dictionary<string, string> m_AliasToServiceNameMap;
        private readonly Stack<IBindingData> m_BindingDatasToBuild;
        private readonly Queue<string> m_ServicesToInit;
        internal readonly MinPriorityQueue<string, ILifeCycle> ServicesToShutdown;
        internal event Action<IBindingData, object> OnInstanceCreated;
        private int m_ServiceInitCounter = 0;

        private bool m_Disposing = false;
        private bool m_Disposed = false;


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
        public bool IsDisposing => m_Disposing;

        /// <inheritdoc />
        public bool IsDisposed => m_Disposed;

        /// <inheritdoc />
        public IBindingData BindSingleton(string serviceName, Type implType)
        {
            GuardNotDisposingOrDisposed();
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
        public IBindingData BindSingleton(string serviceName, Type implType, params PropertyInjection[] propertyInjections)
        {
            GuardNotDisposingOrDisposed();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            GuardUnbound(Dealias(serviceName));
            GuardImplType(implType);
            var bindingData = new BindingData(this)
            {
                ServiceName = serviceName,
                ImplType = implType,
                LifeCycleManaged = true,
            };
            int propertyInjectionIndex = 0;
            foreach (var propertyInjection in propertyInjections)
            {
                if (string.IsNullOrEmpty(propertyInjection.PropertyName))
                {
                    throw new ArgumentException($"Property injection {propertyInjectionIndex} has a invalid property name.");
                }

                if (propertyInjection.Value == null)
                {
                    throw new ArgumentException($"Property injection {propertyInjectionIndex} has a null value.");
                }

                var propertyInfo = implType.GetProperty(propertyInjection.PropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                if (propertyInfo == null)
                {
                    throw new ArgumentException($"Cannot find property named '{propertyInjection.PropertyName}' at index {propertyInjectionIndex}.");
                }

                if (!propertyInfo.PropertyType.IsInstanceOfType(propertyInjection.Value))
                {
                    throw new ArgumentException($"Property injection {propertyInjectionIndex} has a value that doesn't have a feasible type.");
                }

                bindingData.AddPropertyInjection(propertyInjection);
                propertyInjectionIndex++;
            }

            m_ServiceNameToBindingDataMap[serviceName] = bindingData;
            return bindingData;
        }


        /// <inheritdoc />
        public IBindingData BindSingleton(Type interfaceType, Type implType)
        {
            GuardNotDisposingOrDisposed();
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

        public IBindingData BindSingleton(Type interfaceType, Type implType, params PropertyInjection[] propertyInjections)
        {
            GuardNotDisposingOrDisposed();
            GuardInterfaceType(interfaceType);
            GuardImplType(implType);
            if (!interfaceType.IsAssignableFrom(implType))
            {
                throw new InvalidOperationException($"{nameof(interfaceType)} is not assignable from {nameof(implType)}.");
            }

            var bindingData = (BindingData)BindSingleton(TypeToServiceName(interfaceType), implType, propertyInjections);
            bindingData.InterfaceType = interfaceType;
            m_InterfaceTypeToBindingDataMap[interfaceType] = bindingData;
            return bindingData;
        }

        /// <inheritdoc />
        public IBindingData BindInstance(Type interfaceType, object instance)
        {
            GuardNotDisposingOrDisposed();
            GuardInterfaceType(interfaceType);
            Guard.RequireNotNull<ArgumentNullException>(instance, $"Invalid '{nameof(instance)}'.");
            var implType = instance.GetType();
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
            GuardNotDisposingOrDisposed();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            GuardUnbound(Dealias(serviceName));
            var implType = instance.GetType();
            Guard.RequireNotNull<ArgumentNullException>(instance, $"Invalid '{nameof(instance)}'.");
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
            GuardNotDisposingOrDisposed();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            serviceName = Dealias(serviceName);
            return m_ServiceNameToBindingDataMap.TryGetValue(serviceName, out bindingData);
        }

        /// <inheritdoc />
        public bool TryGetBindingData(Type interfaceType, out IBindingData bindingData)
        {
            GuardNotDisposingOrDisposed();
            Guard.RequireNotNull<ArgumentNullException>(interfaceType, $"Invalid '{nameof(interfaceType)}'.");
            return m_InterfaceTypeToBindingDataMap.TryGetValue(interfaceType, out bindingData);
        }

        /// <inheritdoc />
        public bool IsBound(string serviceName)
        {
            GuardNotDisposingOrDisposed();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            return m_ServiceNameToBindingDataMap.ContainsKey(Dealias(serviceName));
        }

        /// <inheritdoc />
        public bool TypeIsBound(Type interfaceType)
        {
            GuardNotDisposingOrDisposed();
            GuardInterfaceType(interfaceType);
            return m_InterfaceTypeToBindingDataMap.ContainsKey(interfaceType);
        }

        /// <inheritdoc />
        public object Make(string serviceName)
        {
            GuardNotDisposingOrDisposed();
            Guard.RequireNotNullOrEmpty<ArgumentException>(serviceName, $"Invalid '{nameof(serviceName)}'.");
            return MakeInternal((BindingData)GetBindingData(serviceName));
        }

        /// <inheritdoc />
        public object Make(Type interfaceType)
        {
            GuardNotDisposingOrDisposed();
            return MakeInternal((BindingData)GetBindingData(interfaceType));
        }

        internal object MakeInternal(BindingData bindingData)
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
                var bindingData = (BindingData)m_ServiceNameToBindingDataMap[serviceNameToInit];

                if (!bindingData.LifeCycleManaged)
                {
                    continue;
                }

                if (!(serviceInstance is ILifeCycle lifeCycleInstance))
                {
                    continue;
                }

                m_ServiceInitCounter++;
                InvokeCallbacks(serviceInstance, bindingData.OnPreInitCallbacks);
                lifeCycleInstance.OnInit();
                InvokeCallbacks(serviceInstance, bindingData.OnPostInitCallbacks);
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
                    if (bindingData.PropertyInjections != null && bindingData.PropertyInjections.TryGetValue(property.Name, out var value))
                    {
                        property.SetValue(ret, value);
                        continue;
                    }

                    if (property.GetCustomAttribute<InjectAttribute>() == null)
                    {
                        continue;
                    }

                    var dependency = ResolveInternal((BindingData)GetBindingData(property.PropertyType));
                    property.SetValue(ret, dependency);
                }

                m_ServiceNameToSingletonMap[bindingData.ServiceName] = ret;
                m_ServicesToInit.Enqueue(bindingData.ServiceName);
                OnInstanceCreated?.Invoke(bindingData, ret);
            }

            m_BindingDatasToBuild.Pop();
            return ret;
        }

        public void Dispose()
        {
            GuardNotDisposingOrDisposed();
            m_Disposing = true;
            while (ServicesToShutdown.Count > 0)
            {
                var node = ServicesToShutdown.Min;
                ShutDown(node.Key);
                ServicesToShutdown.PopMin();
            }

            Clear();
            m_Disposing = false;
            m_Disposed = true;
        }

        public void Alias(string serviceName, string alias)
        {
            GuardNotDisposingOrDisposed();
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
            var bindingData = (BindingData)m_ServiceNameToBindingDataMap[serviceName];
            if (serviceInstance is ILifeCycle lifeCycleInstance)
            {
                InvokeCallbacks(serviceInstance, bindingData.OnPreShutdownCallbacks);
                lifeCycleInstance.OnShutdown();
                InvokeCallbacks(bindingData.OnPostShutdownCallbacks);
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
            GuardNotDisposingOrDisposed();
            return m_AliasToServiceNameMap.TryGetValue(serviceName, out var realServiceName) ? realServiceName : serviceName;
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, object>> GetSingletons()
        {
            foreach (var kv in m_ServiceNameToSingletonMap)
            {
                yield return kv;
            }
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IBindingData>> GetBindingDatas()
        {
            foreach (var kv in m_ServiceNameToBindingDataMap)
            {
                yield return kv;
            }
        }

        internal void GuardNotDisposingOrDisposed()
        {
            Guard.RequireFalse<InvalidOperationException>(m_Disposing || m_Disposed,
                "The container is already disposed or being disposed.");
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

        internal static void InvokeCallbacks(object param, IList<Action<object>> callbackList)
        {
            if (callbackList == null)
            {
                return;
            }

            foreach (var callback in callbackList)
            {
                callback(param);
            }
        }

        internal static void InvokeCallbacks(IList<Action> callbackList)
        {
            if (callbackList == null)
            {
                return;
            }

            foreach (var callback in callbackList)
            {
                callback();
            }
        }
    }
}
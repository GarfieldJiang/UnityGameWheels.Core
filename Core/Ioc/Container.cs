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
        private readonly Dictionary<Type, BindingData> m_InterfaceTypeToBindingDataMap;
        private readonly Dictionary<Type, object> m_InterfaceTypeToSingletonMap;
        private readonly Stack<BindingData> m_BindingDatasToBuild;
        private readonly Queue<Type> m_ServicesToInit;
        internal readonly MinPriorityQueue<Type, ILifeCycle> ServicesToShutdown;
        private int m_ServiceInitCounter = 0;

        private bool m_Disposing = false;
        private bool m_Disposed = false;
        private bool m_HasMadeSomething = false;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="estimatedServiceCount">Estimated service count.</param>
        public Container(int estimatedServiceCount = 64)
        {
            Guard.RequireTrue<ArgumentOutOfRangeException>(estimatedServiceCount > 0,
                $"Argument '{nameof(estimatedServiceCount)}' must be positive.");
            m_InterfaceTypeToBindingDataMap = new Dictionary<Type, BindingData>(estimatedServiceCount);
            m_InterfaceTypeToSingletonMap = new Dictionary<Type, object>(estimatedServiceCount);
            m_BindingDatasToBuild = new Stack<BindingData>(estimatedServiceCount);
            m_ServicesToInit = new Queue<Type>(estimatedServiceCount);
            ServicesToShutdown = new MinPriorityQueue<Type, ILifeCycle>();
        }

        /// <inheritdoc />
        public bool IsDisposing => m_Disposing;

        /// <inheritdoc />
        public bool IsDisposed => m_Disposed;

        private BindingData AddPropertyInjections(BindingData bindingData, params PropertyInjection[] propertyInjections)
        {
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

                var propertyInfo = bindingData.ImplType.GetProperty(propertyInjection.PropertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
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

            return bindingData;
        }


        /// <inheritdoc />
        public IBindingData BindSingleton(Type interfaceType, Type implType)
        {
            GuardHasMadeNothing();
            GuardNotDisposingOrDisposed();
            GuardInterfaceType(interfaceType);
            GuardUnbound(interfaceType);
            GuardImplType(implType);
            if (!interfaceType.IsAssignableFrom(implType))
            {
                throw new InvalidOperationException($"{nameof(interfaceType)} is not assignable from {nameof(implType)}.");
            }

            var bindingData = new BindingData(this)
            {
                InterfaceType = interfaceType,
                ImplType = implType,
                LifeCycleManaged = true,
            };
            m_InterfaceTypeToBindingDataMap[interfaceType] = bindingData;
            return bindingData;
        }

        public IBindingData BindSingleton(Type interfaceType, Type implType, params PropertyInjection[] propertyInjections)
        {
            GuardHasMadeNothing();
            GuardNotDisposingOrDisposed();
            GuardInterfaceType(interfaceType);
            GuardUnbound(interfaceType);
            GuardImplType(implType);
            if (!interfaceType.IsAssignableFrom(implType))
            {
                throw new InvalidOperationException($"{nameof(interfaceType)} is not assignable from {nameof(implType)}.");
            }

            var bindingData = new BindingData(this)
            {
                InterfaceType = interfaceType,
                ImplType = implType,
                LifeCycleManaged = true,
            };

            AddPropertyInjections(bindingData, propertyInjections);
            m_InterfaceTypeToBindingDataMap[interfaceType] = bindingData;
            return bindingData;
        }

        /// <inheritdoc />
        public IBindingData BindInstance(Type interfaceType, object instance)
        {
            GuardHasMadeNothing();
            GuardNotDisposingOrDisposed();
            GuardInterfaceType(interfaceType);
            GuardUnbound(interfaceType);
            Guard.RequireNotNull<ArgumentNullException>(instance, $"Invalid '{nameof(instance)}'.");
            var implType = instance.GetType();
            if (!interfaceType.IsAssignableFrom(implType))
            {
                throw new InvalidOperationException($"{nameof(interfaceType)} is not assignable from {nameof(implType)}.");
            }

            var bindingData = new BindingData(this)
            {
                InterfaceType = interfaceType,
                ImplType = implType,
                LifeCycleManaged = false,
            };
            m_InterfaceTypeToBindingDataMap[interfaceType] = bindingData;
            // TODO: This mixes instances and singletons. Should be redesigned.
            m_InterfaceTypeToSingletonMap[interfaceType] = instance;
            return bindingData;
        }

        public IBindingData GetBindingData(Type interfaceType)
        {
            if (!TryGetBindingData(interfaceType, out var bindingData))
            {
                throw new InvalidOperationException($"Service type '{interfaceType}' is not bound yet.");
            }

            return bindingData;
        }


        public bool TryGetBindingData(Type interfaceType, out IBindingData bindingData)
        {
            GuardNotDisposingOrDisposed();
            Guard.RequireNotNull<ArgumentNullException>(interfaceType, $"Invalid '{nameof(interfaceType)}'.");
            var ret = m_InterfaceTypeToBindingDataMap.TryGetValue(interfaceType, out var internalBindingData);
            bindingData = internalBindingData;
            return ret;
        }

        public bool TypeIsBound(Type interfaceType)
        {
            GuardNotDisposingOrDisposed();
            GuardInterfaceType(interfaceType);
            return m_InterfaceTypeToBindingDataMap.ContainsKey(interfaceType);
        }

        public object Make(Type interfaceType)
        {
            GuardNotDisposingOrDisposed();
            return MakeInternal((BindingData)GetBindingData(interfaceType));
        }

        private object MakeInternal(BindingData bindingData)
        {
            object serviceInstance;
            m_HasMadeSomething = true;
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
                var serviceTypeToInit = m_ServicesToInit.Dequeue();
                var serviceInstance = m_InterfaceTypeToSingletonMap[serviceTypeToInit];
                var bindingData = (BindingData)m_InterfaceTypeToBindingDataMap[serviceTypeToInit];

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
                ServicesToShutdown.Insert(serviceTypeToInit, lifeCycleInstance, -m_ServiceInitCounter);
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

            if (!m_InterfaceTypeToSingletonMap.TryGetValue(bindingData.InterfaceType, out object ret))
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

                m_InterfaceTypeToSingletonMap[bindingData.InterfaceType] = ret;
                m_ServicesToInit.Enqueue(bindingData.InterfaceType);
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


        public string TypeToServiceName(Type interfaceType)
        {
            Guard.RequireNotNull<ArgumentNullException>(interfaceType, $"Invalid '{nameof(interfaceType)}.");
            return interfaceType.ToString();
        }

        private bool ShutDown(Type interfaceType)
        {
            if (!m_InterfaceTypeToSingletonMap.TryGetValue(interfaceType, out var serviceInstance))
            {
                return false;
            }

            m_InterfaceTypeToSingletonMap.Remove(interfaceType);
            var bindingData = m_InterfaceTypeToBindingDataMap[interfaceType];
            if (serviceInstance is ILifeCycle lifeCycleInstance)
            {
                InvokeCallbacks(serviceInstance, bindingData.OnPreShutdownCallbacks);
                lifeCycleInstance.OnShutdown();
                InvokeCallbacks(bindingData.OnPostShutdownCallbacks);
            }

            return true;
        }

        private void Clear()
        {
            m_BindingDatasToBuild.Clear();
            m_InterfaceTypeToSingletonMap.Clear();
            m_InterfaceTypeToBindingDataMap.Clear();
        }

        public IEnumerable<KeyValuePair<Type, object>> GetSingletons()
        {
            foreach (var kv in m_InterfaceTypeToSingletonMap)
            {
                yield return kv;
            }
        }

        public IEnumerable<KeyValuePair<Type, IBindingData>> GetBindingDatas()
        {
            foreach (var kv in m_InterfaceTypeToBindingDataMap)
            {
                yield return new KeyValuePair<Type, IBindingData>(kv.Key, kv.Value);
            }
        }

        private void GuardNotDisposingOrDisposed()
        {
            Guard.RequireFalse<InvalidOperationException>(m_Disposing || m_Disposed,
                "The container is already disposed or being disposed.");
        }

        private void GuardHasMadeNothing()
        {
            Guard.RequireFalse<InvalidOperationException>(m_HasMadeSomething,
                "The container has already made something.");
        }

        private void GuardImplType(Type implType)
        {
            Guard.RequireNotNull<ArgumentNullException>(implType, $"Invalid {nameof(implType)}.");
            Guard.RequireTrue<ArgumentException>(implType.IsClass && !implType.IsAbstract && !implType.IsInterface && !implType.IsGenericTypeDefinition
                                                 && implType.GetConstructors().Any(c => c.GetParameters().Length == 0),
                $"{nameof(implType)} '{implType}' is not supported");
        }

        private void GuardInterfaceType(Type interfaceType)
        {
            Guard.RequireNotNull<ArgumentNullException>(interfaceType, $"Invalid {nameof(interfaceType)}.");
            Guard.RequireTrue<ArgumentException>((!interfaceType.IsAbstract || !interfaceType.IsSealed) && !interfaceType.IsGenericTypeDefinition,
                $"{nameof(interfaceType)} '{interfaceType}' is not supported.");
        }

        private void GuardUnbound(Type interfaceType)
        {
            Guard.RequireFalse<InvalidOperationException>(m_InterfaceTypeToBindingDataMap.ContainsKey(interfaceType),
                $"{nameof(interfaceType)} '{interfaceType}' already bound.");
        }


        private static void InvokeCallbacks(object param, IList<Action<object>> callbackList)
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

        private static void InvokeCallbacks(IList<Action> callbackList)
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
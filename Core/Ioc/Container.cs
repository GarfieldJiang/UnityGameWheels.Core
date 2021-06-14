using System;
using System.Collections.Generic;
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
        private readonly Dictionary<Type, object> m_InterfaceTypeToInstanceMap;
        private readonly Stack<BindingData> m_BindingDatasToBuild;
        private readonly MinPriorityQueue<Type, IDisposable> ServicesToDispose;
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
            m_InterfaceTypeToSingletonMap = new Dictionary<Type, object>(estimatedServiceCount >> 1);
            m_InterfaceTypeToInstanceMap = new Dictionary<Type, object>(estimatedServiceCount >> 1);
            m_BindingDatasToBuild = new Stack<BindingData>(estimatedServiceCount);
            ServicesToDispose = new MinPriorityQueue<Type, IDisposable>();
        }

        public bool IsDisposing => m_Disposing;

        public bool IsDisposed => m_Disposed;


        private BindingData BindInternal(Type interfaceType, Type implType, ILifeStyle lifeStyle)
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
                LifeStyle = lifeStyle,
            };
            m_InterfaceTypeToBindingDataMap[interfaceType] = bindingData;
            return bindingData;
        }

        public IBindingData Bind(Type interfaceType, Type implType)
        {
            var bindingData = BindInternal(interfaceType, implType, LifeStyles.Transient);
            return bindingData;
        }


        public IBindingData BindSingleton(Type interfaceType, Type implType)
        {
            var bindingData = BindInternal(interfaceType, implType, LifeStyles.Singleton);
            return bindingData;
        }

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
                LifeStyle = LifeStyles.Null,
            };
            m_InterfaceTypeToBindingDataMap[interfaceType] = bindingData;
            m_InterfaceTypeToInstanceMap[interfaceType] = instance;
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
            }
            finally
            {
                m_BindingDatasToBuild.Clear();
            }

            return serviceInstance;
        }

        private object ResolveConstructorParametersAndCreateInstance(Type instanceType, ParameterInfo[] parameterInfos)
        {
            var dependencies = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                dependencies[i] = ResolveInternal((BindingData)GetBindingData(parameterInfos[i].ParameterType));
            }

            return Activator.CreateInstance(instanceType, dependencies);
        }

        private object DoConstructorStuff(BindingData bindingData)
        {
            var instanceType = bindingData.ImplType;
            if (!bindingData.HasCachedConstructorParameterInfos)
            {
                bindingData.HasCachedConstructorParameterInfos = true;
                var defaultConstructorInfo = instanceType.GetConstructor(Type.EmptyTypes);
                if (defaultConstructorInfo != null && defaultConstructorInfo.IsPublic)
                {
                    bindingData.ConstructorParameterInfos = new ParameterInfo[0];
                }
                else
                {
                    ParameterInfo[] parameterInfos = null;
                    foreach (var constructorInfo in instanceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var injectable = true;
                        var currentParameterInfos = constructorInfo.GetParameters();
                        foreach (var parameterInfo in currentParameterInfos)
                        {
                            if (TypeIsBound(parameterInfo.ParameterType)) continue;
                            injectable = false;
                            break;
                        }

                        if (!injectable) continue;
                        parameterInfos = currentParameterInfos;
                        break;
                    }

                    bindingData.ConstructorParameterInfos = parameterInfos;
                }
            }

            if (bindingData.ConstructorParameterInfos == null)
            {
                throw new InvalidOperationException($"Implementation type '{instanceType}' doesn't have a constructor that can be used for auto-wiring.");
            }

            return bindingData.ConstructorParameterInfos.Length == 0
                ? Activator.CreateInstance(instanceType)
                : ResolveConstructorParametersAndCreateInstance(instanceType, bindingData.ConstructorParameterInfos);
        }

        private void DoPropertyStuff(BindingData bindingData, object instance)
        {
            foreach (var property in bindingData.ImplType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
            {
                if (property.GetSetMethod() == null)
                {
                    continue;
                }

                if (bindingData.PropertyInjections != null && bindingData.PropertyInjections.TryGetValue(property.Name, out var value))
                {
                    property.SetValue(instance, value);
                    continue;
                }

                if (property.GetCustomAttribute<InjectAttribute>() == null)
                {
                    continue;
                }

                var dependency = ResolveInternal((BindingData)GetBindingData(property.PropertyType));
                property.SetValue(instance, dependency);
            }
        }

        private object ResolveInternal(BindingData bindingData)
        {
            if (m_BindingDatasToBuild.Contains(bindingData))
            {
                throw new InvalidOperationException("Cyclic dependencies are not supported.");
            }

            if (m_InterfaceTypeToInstanceMap.TryGetValue(bindingData.InterfaceType, out var ret))
            {
                return ret;
            }

            var isSingleton = bindingData.LifeStyle == LifeStyles.Singleton;
            if (isSingleton && m_InterfaceTypeToSingletonMap.TryGetValue(bindingData.InterfaceType, out ret))
            {
                return ret;
            }

            if (!bindingData.LifeStyle.AutoCreateInstance)
            {
                throw new InvalidOperationException("Oops!");
            }

            m_BindingDatasToBuild.Push(bindingData);
            ret = DoConstructorStuff(bindingData);
            DoPropertyStuff(bindingData, ret);
            if (isSingleton)
            {
                m_InterfaceTypeToSingletonMap[bindingData.InterfaceType] = ret;
                if (ret is IDisposable disposable)
                {
                    m_ServiceInitCounter++;
                    ServicesToDispose.Insert(bindingData.InterfaceType, disposable, -m_ServiceInitCounter);
                }
            }

            InvokeCallbacks(ret, bindingData.OnInstanceCreatedCallbacks);
            m_BindingDatasToBuild.Pop();
            return ret;
        }

        public void Dispose()
        {
            GuardNotDisposingOrDisposed();
            m_Disposing = true;
            while (ServicesToDispose.Count > 0)
            {
                var node = ServicesToDispose.Min;
                DisposeService(node.Key);
                ServicesToDispose.PopMin();
            }

            Clear();
            m_Disposing = false;
            m_Disposed = true;
        }

        private bool DisposeService(Type interfaceType)
        {
            if (!m_InterfaceTypeToSingletonMap.TryGetValue(interfaceType, out var serviceInstance))
            {
                return false;
            }

            m_InterfaceTypeToSingletonMap.Remove(interfaceType);
            var bindingData = m_InterfaceTypeToBindingDataMap[interfaceType];

            if (!(serviceInstance is IDisposable disposable)) return true;
            InvokeCallbacks(serviceInstance, bindingData.OnPreDisposeCallbacks);
            disposable.Dispose();
            InvokeCallbacks(bindingData.OnDisposedCallbacks);

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

        public IEnumerable<KeyValuePair<Type, object>> GetInstances()
        {
            foreach (var kv in m_InterfaceTypeToInstanceMap)
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
            Guard.RequireTrue<ArgumentException>(
                implType.IsClass && !implType.IsAbstract && !implType.IsInterface && !implType.IsGenericTypeDefinition && implType != typeof(string),
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
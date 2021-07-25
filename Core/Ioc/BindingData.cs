using System;
using System.Collections.Generic;
using System.Reflection;

namespace COL.UnityGameWheels.Core.Ioc
{
    internal class BindingData : IBindingData
    {
        private readonly Container m_Container;

        public Type InterfaceType { get; internal set; }

        public Type ImplType { get; internal set; }

        public ILifeStyle LifeStyle { get; internal set; }

        internal Dictionary<string, object> PropertyInjections;

        private Action<object> OnInstanceCreatedCallback;
        private Action<object> OnPreDisposeCallback;
        private Action OnDisposedCallback;

        internal bool HasCachedConstructorInfo;
        internal ParameterInfo[] ConstructorParameterInfos;
        internal ConstructorInfo ConstructorInfoFromSet;
        internal ConstructorInfo CachedConstructorInfo;

        internal BindingData(Container container)
        {
            m_Container = container;
        }


        private void AddPropertyInjection(PropertyInjection propertyInjection)
        {
            if (PropertyInjections == null)
            {
                PropertyInjections = new Dictionary<string, object>();
            }

            PropertyInjections.Add(propertyInjection.PropertyName, propertyInjection.Value);
        }

        public IBindingData SetConstructor(params Type[] paramTypes)
        {
            if (ConstructorInfoFromSet != null)
            {
                throw new InvalidOperationException("Already set constructor.");
            }

            if (HasCachedConstructorInfo)
            {
                throw new InvalidOperationException("Already cached constructor parameter infos.");
            }

            // TODO: stricter type check.

            bool found = false;
            foreach (var constructorInfo in ImplType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var currentParameterInfos = constructorInfo.GetParameters();

                // Check parameter count.
                if (currentParameterInfos.Length != paramTypes.Length)
                {
                    continue;
                }

                // Match parameter type.
                var veto = false;
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (paramTypes[i] == currentParameterInfos[i].ParameterType) continue;
                    veto = true;
                    break;
                }

                if (!veto)
                {
                    ConstructorInfoFromSet = constructorInfo;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException($"No constructor is found to match '{nameof(paramTypes)}'.");
            }

            return this;
        }

        public IBindingData AddPropertyInjections(params PropertyInjection[] propertyInjections)
        {
            if (!LifeStyle.AutoCreateInstance)
            {
                throw new InvalidOperationException("The binding's life style doesn't support auto instance creation.");
            }

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

                var propertyInfo = ImplType.GetProperty(propertyInjection.PropertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                if (propertyInfo == null)
                {
                    throw new ArgumentException($"Cannot find property named '{propertyInjection.PropertyName}' at index {propertyInjectionIndex}.");
                }

                if (!propertyInfo.PropertyType.IsInstanceOfType(propertyInjection.Value))
                {
                    throw new ArgumentException($"Property injection {propertyInjectionIndex} has a value that doesn't have a feasible type.");
                }

                AddPropertyInjection(propertyInjection);
                propertyInjectionIndex++;
            }

            return this;
        }

        public IBindingData OnInstanceCreated(Action<object> callback)
        {
            if (!LifeStyle.AutoCreateInstance)
            {
                throw new InvalidOperationException("The binding's life style doesn't support auto instance creation.");
            }

            OnInstanceCreatedCallback += callback;
            return this;
        }

        public IBindingData OnPreDispose(Action<object> callback)
        {
            if (!LifeStyle.AutoDispose)
            {
                throw new InvalidOperationException("The binding's life style doesn't support auto disposal.");
            }

            OnPreDisposeCallback += callback;
            return this;
        }

        public IBindingData OnDisposed(Action callback)
        {
            if (!LifeStyle.AutoDispose)
            {
                throw new InvalidOperationException("The binding's life style doesn't support auto disposal.");
            }

            OnDisposedCallback += callback;
            return this;
        }

        internal void InvokeOnInstanceCreatedCallback(object serviceInstance)
        {
            OnInstanceCreatedCallback?.Invoke(serviceInstance);
        }

        internal void InvokeOnPreDisposeCallback(object serviceInstance)
        {
            OnPreDisposeCallback?.Invoke(serviceInstance);
        }

        internal void InvokeOnDisposedCallback()
        {
            OnDisposedCallback?.Invoke();
        }
    }
}
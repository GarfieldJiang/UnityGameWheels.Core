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


        internal bool HasCachedConstructorParameterInfos;
        internal ParameterInfo[] ConstructorParameterInfos;

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
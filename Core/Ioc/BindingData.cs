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

        internal ILifeStyle LifeStyle { get; set; }

        internal Dictionary<string, object> PropertyInjections;

        internal List<Action<object>> OnInstanceCreatedCallbacks;
        internal List<Action<object>> OnPreDisposeCallbacks;
        internal List<Action> OnDisposedCallbacks;

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
            if (LifeStyle.AutoCreateInstance)
            {
                throw new InvalidOperationException("The binding's life style doesn't support auto instance creation.");
            }

            AddCallback(callback, ref OnInstanceCreatedCallbacks);
            return this;
        }

        public IBindingData OnPreDispose(Action<object> callback)
        {
            if (LifeStyle.AutoDispose)
            {
                throw new InvalidOperationException("The binding's life style doesn't support auto disposal.");
            }

            AddCallback(callback, ref OnPreDisposeCallbacks);
            return this;
        }

        public IBindingData OnDisposed(Action callback)
        {
            if (LifeStyle.AutoDispose)
            {
                throw new InvalidOperationException("The binding's life style doesn't support auto disposal.");
            }

            AddCallback(callback, ref OnDisposedCallbacks);
            return this;
        }


        private void AddCallback(Action<object> callback, ref List<Action<object>> callbackList)
        {
            Guard.RequireNotNull<ArgumentNullException>(callback, $"Invalid '{nameof(callback)}'.");

            if (callbackList == null)
            {
                callbackList = new List<Action<object>>();
            }

            callbackList.Add(callback);
        }

        private void AddCallback(Action callback, ref List<Action> callbackList)
        {
            Guard.RequireNotNull<ArgumentNullException>(callback, $"Invalid '{nameof(callback)}'.");

            if (callbackList == null)
            {
                callbackList = new List<Action>();
            }

            callbackList.Add(callback);
        }
    }
}
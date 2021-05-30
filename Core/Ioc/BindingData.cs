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

        public bool LifeCycleManaged { get; internal set; }

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


        internal void AddPropertyInjection(PropertyInjection propertyInjection)
        {
            if (PropertyInjections == null)
            {
                PropertyInjections = new Dictionary<string, object>();
            }

            PropertyInjections.Add(propertyInjection.PropertyName, propertyInjection.Value);
        }

        public IBindingData OnInstanceCreated(Action<object> callback)
        {
            AddCallback(callback, ref OnInstanceCreatedCallbacks);
            return this;
        }

        public IBindingData OnPreDispose(Action<object> callback)
        {
            if (!LifeCycleManaged)
            {
                throw new InvalidOperationException("The binding's life cycle is not managed by the container.");
            }

            AddCallback(callback, ref OnPreDisposeCallbacks);
            return this;
        }

        public IBindingData OnDisposed(Action callback)
        {
            if (!LifeCycleManaged)
            {
                throw new InvalidOperationException("The binding's life cycle is not managed by the container.");
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
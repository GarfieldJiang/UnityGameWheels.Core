using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Ioc
{
    internal class BindingData : IBindingData
    {
        private readonly Container m_Container;

        public Type InterfaceType { get; internal set; }

        public Type ImplType { get; internal set; }

        public bool LifeCycleManaged { get; internal set; }

        internal Dictionary<string, object> PropertyInjections;
        internal List<Action<object>> OnPreInitCallbacks;
        internal List<Action<object>> OnPostInitCallbacks;
        internal List<Action<object>> OnPreShutdownCallbacks;
        internal List<Action> OnPostShutdownCallbacks;

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

        public IBindingData OnPreInit(Action<object> callback)
        {
            AddCallback(callback, ref OnPreInitCallbacks);
            return this;
        }

        public IBindingData OnPostInit(Action<object> callback)
        {
            AddCallback(callback, ref OnPostInitCallbacks);
            return this;
        }

        public IBindingData OnPreShutdown(Action<object> callback)
        {
            AddCallback(callback, ref OnPreShutdownCallbacks);
            return this;
        }

        public IBindingData OnPostShutdown(Action callback)
        {
            if (!typeof(ILifeCycle).IsAssignableFrom(ImplType))
            {
                throw new InvalidOperationException($"The binding's implementation is not {nameof(ILifeCycle)}.");
            }

            if (!LifeCycleManaged)
            {
                throw new InvalidOperationException("The binding's life cycle is not managed by the container.");
            }

            Guard.RequireNotNull<ArgumentNullException>(callback, $"Invalid '{nameof(callback)}'.");

            if (OnPostShutdownCallbacks == null)
            {
                OnPostShutdownCallbacks = new List<Action>();
            }

            OnPostShutdownCallbacks.Add(callback);
            return this;
        }

        private void AddCallback(Action<object> callback, ref List<Action<object>> callbackList)
        {
            if (!typeof(ILifeCycle).IsAssignableFrom(ImplType))
            {
                throw new InvalidOperationException($"The binding's implementation is not {nameof(ILifeCycle)}.");
            }

            if (!LifeCycleManaged)
            {
                throw new InvalidOperationException("The binding's life cycle is not managed by the container.");
            }

            Guard.RequireNotNull<ArgumentNullException>(callback, $"Invalid '{nameof(callback)}'.");

            if (callbackList == null)
            {
                callbackList = new List<Action<object>>();
            }

            callbackList.Add(callback);
        }
    }
}
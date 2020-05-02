using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Ioc
{
    internal class BindingData : IBindingData
    {
        private readonly IContainer m_Container;

        public Type InterfaceType { get; internal set; }

        public Type ImplType { get; internal set; }

        public string ServiceName { get; internal set; }

        public bool LifeCycleManaged { get; internal set; }

        internal HashSet<string> Aliases;

        internal BindingData(IContainer container)
        {
            m_Container = container;
        }

        public IBindingData Alias(string alias)
        {
            m_Container.Alias(ServiceName, alias);
            return this;
        }

        internal bool AliasInternal(string alias)
        {
            if (Aliases == null)
            {
                Aliases = new HashSet<string>();
            }

            return Aliases.Add(alias);
        }
    }
}
using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Ioc
{
    internal class BindingData : IBindingData
    {
        private IContainer m_Container;

        internal Type InterfaceType;

        internal Type ImplType;

        internal string ServiceName;

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
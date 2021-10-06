namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Extension method for <see cref="Container"/>.
    /// </summary>
    public static class ContainerExtension
    {
        public static IBindingData Bind<TService>(this Container container)
            where TService : class
        {
            var type = typeof(TService);
            return container.Bind(type, type);
        }

        public static IBindingData Bind<TInterface, TImpl>(this Container container)
            where TInterface : class
            where TImpl : class
        {
            return container.Bind(typeof(TInterface), typeof(TImpl));
        }

        public static IBindingData BindSingleton<TService>(this Container container)
            where TService : class
        {
            var type = typeof(TService);
            return container.BindSingleton(type, type);
        }

        public static IBindingData BindSingleton<TInterface, TImpl>(this Container container)
            where TInterface : class
            where TImpl : class
        {
            return container.BindSingleton(typeof(TInterface), typeof(TImpl));
        }

        public static IBindingData BindInstance<TInterface>(this Container container, TInterface instance)
            where TInterface : class
        {
            return container.BindInstance(typeof(TInterface), instance);
        }

        public static TService Make<TService>(this Container container)
            where TService : class
        {
            return (TService)container.Make(typeof(TService));
        }

        public static bool IsBound<TService>(this Container container)
            where TService : class
        {
            return container.TypeIsBound(typeof(TService));
        }
    }
}
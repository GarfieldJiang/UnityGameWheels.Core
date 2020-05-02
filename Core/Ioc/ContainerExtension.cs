namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Extension method for <see cref="IContainer"/>.
    /// </summary>
    public static class ContainerExtension
    {
        public static IBindingData BindSingleton<TService>(this IContainer container)
            where TService : class, new()
        {
            var type = typeof(TService);
            return container.BindSingleton(type, type);
        }

        public static IBindingData BindSingleton<TInterface, TImpl>(this IContainer container)
            where TInterface : class
            where TImpl : class, new()
        {
            return container.BindSingleton(typeof(TInterface), typeof(TImpl));
        }

        public static TService Make<TService>(this IContainer container)
            where TService : class
        {
            return (TService)container.Make(typeof(TService));
        }

        public static bool IsBound<TService>(this IContainer container)
            where TService : class
        {
            return container.TypeIsBound(typeof(TService));
        }
    }
}
namespace COL.UnityGameWheels.Core.Ioc
{
    internal interface ILifeStyle
    {
        bool AutoCreateInstance { get; }
        bool AutoDispose { get; }
    }

    internal static class LifeStyles
    {
        public static ILifeStyle Null = new NullLifeStyle();
        public static ILifeStyle Singleton = new SingletonLifeStyle();
        public static ILifeStyle Transient = new TransientLifeCycle();
    }

    internal class NullLifeStyle : ILifeStyle
    {
        internal NullLifeStyle()
        {
        }

        public bool AutoCreateInstance => false;
        public bool AutoDispose => false;
    }

    internal class SingletonLifeStyle : ILifeStyle
    {
        internal SingletonLifeStyle()
        {
        }

        public bool AutoCreateInstance => true;
        public bool AutoDispose => true;
    }

    internal class TransientLifeCycle : ILifeStyle
    {
        internal TransientLifeCycle()
        {
        }

        public bool AutoCreateInstance => true;
        public bool AutoDispose => false;
    }
}
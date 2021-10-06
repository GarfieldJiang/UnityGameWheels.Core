namespace COL.UnityGameWheels.Core.Ioc
{
    public interface ILifeStyle
    {
        bool AutoCreateInstance { get; }
        bool AutoDispose { get; }
    }

    public static class LifeStyles
    {
        public static ILifeStyle Null = new NullLifeStyle();
        public static ILifeStyle Singleton = new SingletonLifeStyle();
        public static ILifeStyle Transient = new TransientLifeCycle();
    }

    public class NullLifeStyle : ILifeStyle
    {
        internal NullLifeStyle()
        {
        }

        public bool AutoCreateInstance => false;
        public bool AutoDispose => false;
    }

    public class SingletonLifeStyle : ILifeStyle
    {
        internal SingletonLifeStyle()
        {
        }

        public bool AutoCreateInstance => true;
        public bool AutoDispose => true;
    }

    public class TransientLifeCycle : ILifeStyle
    {
        internal TransientLifeCycle()
        {
        }

        public bool AutoCreateInstance => true;
        public bool AutoDispose => false;
    }
}
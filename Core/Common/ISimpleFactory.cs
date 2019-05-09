namespace COL.UnityGameWheels.Core
{
    public interface ISimpleFactory<T> where T : class
    {
        T Get();
    }
}

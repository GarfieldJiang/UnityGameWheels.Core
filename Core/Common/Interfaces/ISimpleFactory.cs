namespace COL.UnityGameWheels.Core
{
    public interface ISimpleFactory<out T> where T : class
    {
        T Get();
    }
}

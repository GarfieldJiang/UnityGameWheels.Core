namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface to destroy an object.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    public interface IObjectDestroyer<T> where T : class
    {
        /// <summary>
        /// Destroy the given object.
        /// </summary>
        /// <param name="obj">Object to destroy.</param>
        void Destroy(T obj);
    }
}

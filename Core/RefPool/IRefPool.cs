namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Generic reference pool interface.
    /// </summary>
    /// <typeparam name="TObject">Object type.</typeparam>
    public interface IRefPool<TObject> : IBaseRefPool
        where TObject : class, new()
    {
        /// <summary>
        /// Acquire an object from this pool.
        /// </summary>
        /// <returns>The object.</returns>
        TObject Acquire();

        /// <summary>
        /// Release an object back to this pool.
        /// </summary>
        /// <param name="obj">The object to release.</param>
        void Release(TObject obj);
    }
}
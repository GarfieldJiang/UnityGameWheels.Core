namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Abstraction of a simple life cycle.
    /// </summary>
    public interface ILifeCycle
    {
        /// <summary>
        /// Callback on initialization.
        /// </summary>
        void OnInit();

        /// <summary>
        /// Callback on shutdown.
        /// </summary>
        void OnShutdown();
    }
}
namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface for event args releasing.
    /// </summary>
    public interface IEventArgsReleaser
    {
        /// <summary>
        /// Release a <see cref="BaseEventArgs"/> instance.
        /// </summary>
        /// <param name="eventArgs"></param>
        void Release(BaseEventArgs eventArgs);
    }
}
namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Base class of events.
    /// </summary>
    public abstract class BaseEventArgs
    {
        /// <summary>
        /// Event identifier. Each type of event should have the same identifier among all its instances.
        /// </summary>
        public abstract int EventId { get; }
    }
}
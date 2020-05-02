namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Abstraction of a tickable class.
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// Callback on tick.
        /// </summary>
        /// <param name="timeStruct">The time struct.</param>
        void OnUpdate(TimeStruct timeStruct);
    }
}
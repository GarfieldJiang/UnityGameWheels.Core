namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface for a module.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Initialize this module.
        /// </summary>
        void Init();

        /// <summary>
        /// Generic tick method.
        /// </summary>
        /// <param name="timeStruct">Time struct.</param>
        void Update(TimeStruct timeStruct);

        /// <summary>
        /// Shut down this module.
        /// </summary>
        void ShutDown();
    }
}
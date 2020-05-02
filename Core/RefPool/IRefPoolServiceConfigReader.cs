namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface of reference pool service config reader.
    /// </summary>
    public interface IRefPoolServiceConfigReader : IConfigReader
    {
        /// <summary>
        /// Default capacity of any pool.
        /// </summary>
        int DefaultCapacity { get; }
    }
}
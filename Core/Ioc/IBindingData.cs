namespace COL.UnityGameWheels.Core.Ioc
{
    /// <summary>
    /// Interface for binding data.
    /// </summary>
    public interface IBindingData
    {
        /// <summary>
        /// Add an alias of the current binding.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>Self.</returns>
        IBindingData Alias(string alias);
    }
}
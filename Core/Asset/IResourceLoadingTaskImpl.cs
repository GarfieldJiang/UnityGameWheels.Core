namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Resource loading task implentation interface.
    /// </summary>
    public interface IResourceLoadingTaskImpl : ITask
    {
        /// <summary>
        /// The resource path.
        /// </summary>
        string ResourcePath { get; set; }

        /// <summary>
        /// The parent directory of resources.
        /// </summary>
        string ResourceParentDir { get; set; }

        /// <summary>
        /// The loaded resource object.
        /// </summary>
        object ResourceObject { get; }
    }
}
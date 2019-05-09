namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Resource group status.
    /// </summary>
    public enum ResourceGroupStatus
    {
        /// <summary>
        /// The resource group needs updating.
        /// </summary>
        OutOfDate,

        /// <summary>
        /// The resource group is being updated.
        /// </summary>
        BeingUpdated,

        /// <summary>
        /// The resource group is up-to-date.
        /// </summary>
        UpToDate,
    }
}

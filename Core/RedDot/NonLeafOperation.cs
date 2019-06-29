namespace COL.UnityGameWheels.Core.RedDot
{
    /// <summary>
    /// Operations non-leaf nodes can take.
    /// </summary>
    public enum NonLeafOperation
    {
        /// <summary>
        /// Calculate the value as OR result of all keys this non-leaf node depends on.
        /// </summary>
        Or,

        /// <summary>
        /// Calculate the value as sum of all keys this non-leaf node depends on.
        /// </summary>
        Sum,
    }
}
namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Callback when asset update checking succeeds.
    /// </summary>
    /// <param name="context">Context.</param>
    public delegate void OnUpdateCheckSuccess(object context);

    /// <summary>
    /// Callback when asset update checking fails.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="context">Context.</param>
    public delegate void OnUpdateCheckFailure(string errorMessage, object context);

    /// <summary>
    /// Update checking callback set.
    /// </summary>
    public struct UpdateCheckCallbackSet
    {
        /// <summary>
        /// On success callback.
        /// </summary>
        public OnUpdateCheckSuccess OnSuccess;

        /// <summary>
        /// On failure callback.
        /// </summary>
        public OnUpdateCheckFailure OnFailure;
    }
}

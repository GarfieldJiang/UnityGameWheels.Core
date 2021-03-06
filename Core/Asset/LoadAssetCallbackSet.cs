﻿namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// On load asset success.
    /// </summary>
    public delegate void OnLoadAssetSuccess(IAssetAccessor assetAccessor, object context);

    /// <summary>
    /// On load asset failure.
    /// </summary>
    public delegate void OnLoadAssetFailure(IAssetAccessor assetAccessor, string errorMessage, object context);

    /// <summary>
    /// On load asset progress.
    /// </summary>
    public delegate void OnLoadAssetProgress(IAssetAccessor assetAccessor, float progress, object context);

    /// <summary>
    /// Load asset callback set.
    /// </summary>
    public struct LoadAssetCallbackSet
    {
        /// <summary>
        /// On success callback.
        /// </summary>
        public OnLoadAssetSuccess OnSuccess;

        /// <summary>
        /// On failure callback.
        /// </summary>
        public OnLoadAssetFailure OnFailure;

        /// <summary>
        /// On progress callback.
        /// </summary>
        public OnLoadAssetProgress OnProgress;
    }
}
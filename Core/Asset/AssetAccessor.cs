namespace COL.UnityGameWheels.Core.Asset
{
    using System;
    using AssetCache = AssetModule.Loader.AssetCache;

    /// <summary>
    /// Asset accessor.
    /// </summary>
    internal class AssetAccessor : IAssetAccessor
    {
        private static int s_SerialId = 0;

        private static int NextSerialId() => ++s_SerialId;

        private int m_SerialId = 0;

        internal LoadAssetCallbackSet CallbackSet { get; private set; }

        internal object Context;

        private AssetCache m_AssetCache = null;

        public object AssetObject => m_AssetCache?.AssetObject;

        public string AssetPath => m_AssetCache == null ? string.Empty : m_AssetCache.Path;

        public AssetAccessorStatus Status
        {
            get
            {
                if (m_AssetCache == null)
                {
                    return AssetAccessorStatus.None;
                }

                switch (m_AssetCache.Status)
                {
                    case AssetCacheStatus.Ready:
                        return AssetAccessorStatus.Ready;
                    case AssetCacheStatus.Failure:
                        return AssetAccessorStatus.Failure;
                    case AssetCacheStatus.Loading:
                    case AssetCacheStatus.WaitingForDeps:
                    case AssetCacheStatus.WaitingForSlot:
                    case AssetCacheStatus.WaitingForResource:
                        return AssetAccessorStatus.Loading;
                    case AssetCacheStatus.None:
                    default:
                        return AssetAccessorStatus.None;
                }
            }
        }

        internal void Init(AssetCache assetCache, LoadAssetCallbackSet callbackSet, object context)
        {
            if (m_SerialId > 0)
            {
                throw new InvalidOperationException("Oops, I'm not reset.");
            }

            m_AssetCache = assetCache ?? throw new ArgumentNullException(nameof(assetCache), "Oops!");
            m_SerialId = NextSerialId();

            m_AssetCache.IncreaseRetainCount();

            CallbackSet = callbackSet;
            Context = context;
            m_AssetCache.AddAccessor(this);
        }

        internal void Reset()
        {
            if (m_SerialId <= 0)
            {
                throw new InvalidOperationException("Already reset.");
            }

            m_AssetCache.RemoveAccessor(this);
            ResetCallbacks();

            m_AssetCache.ReduceRetainCount();
            m_AssetCache = null;
            m_SerialId = 0;
        }

        internal void ResetCallbacks()
        {
            CallbackSet = default(LoadAssetCallbackSet);
            Context = null;
        }
    }
}
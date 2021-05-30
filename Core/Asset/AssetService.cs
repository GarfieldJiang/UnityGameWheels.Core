using System;
using System.Collections.Generic;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public sealed partial class AssetService : TickableService, IAssetService
    {
        private readonly IDownloadService m_DownloadService = null;
        private readonly IRefPoolService m_RefPoolService = null;
        private readonly ISimpleFactory<IAssetLoadingTaskImpl> m_AssetLoadingTaskImplFactory = null;
        private readonly ISimpleFactory<IResourceLoadingTaskImpl> m_ResourceLoadingTaskImplFactory = null;
        private int? m_ConcurrentAssetLoaderCount = null;
        private int? m_ConcurrentResourceLoaderCount = null;
        private int? m_AssetCachePoolCapacity = null;
        private int? m_ResourceCachePoolCapacity = null;
        private int? m_AssetAccessorPoolCapacity = null;
        private int? m_DownloadRetryCount = null;
        private bool? m_UpdateIsEnabled = null;
        private string m_UpdateRelativePathFormat = null;
        private string m_BundleVersion = null;
        private string m_ReadWritePath = null;
        private string m_InstallerPath = null;
        private string m_RunningPlatform = null;
        private string m_InstallerIndexPath = null;
        private string m_ReadWriteIndexPath = null;
        private string m_CachedRemoteIndexPath = null;
        private readonly List<Uri> m_UpdateServerRootUrls = new List<Uri>();
        private int? m_UpdateSizeBeforeSavingReadWriteIndex = null;

        private readonly IAssetIndexForInstallerLoader m_AssetIndexForInstallerLoader = null;
        private readonly IObjectDestroyer<object> m_ResourceDestroyer = null;

        private readonly AssetIndexForInstaller m_InstallerIndex = new AssetIndexForInstaller();
        private readonly AssetIndexForReadWrite m_ReadWriteIndex = new AssetIndexForReadWrite();
        private readonly AssetIndexForRemote m_RemoteIndex = new AssetIndexForRemote();

        private readonly Preparer m_Preparer = null;
        private readonly UpdateChecker m_UpdateChecker = null;
        private readonly Updater m_Updater = null;
        private readonly Loader m_Loader = null;

        // Initialized by the update checker and used by the updater.
        private readonly Dictionary<int, ResourceGroupUpdateSummary> ResourceGroupUpdateSummaries =
            new Dictionary<int, ResourceGroupUpdateSummary>();

        public int ConcurrentAssetLoaderCount
        {
            get
            {
                if (m_ConcurrentAssetLoaderCount == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_ConcurrentAssetLoaderCount.Value;
            }
            set
            {
                if (m_ConcurrentAssetLoaderCount != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_ConcurrentAssetLoaderCount = value;
            }
        }

        public int ConcurrentResourceLoaderCount
        {
            get
            {
                if (m_ConcurrentResourceLoaderCount == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_ConcurrentResourceLoaderCount.Value;
            }
            set
            {
                if (m_ConcurrentResourceLoaderCount != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_ConcurrentResourceLoaderCount = value;
            }
        }

        public int DownloadRetryCount
        {
            get
            {
                if (m_DownloadRetryCount == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_DownloadRetryCount.Value;
            }
            set
            {
                if (m_DownloadRetryCount != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be non-negative.");
                }

                m_DownloadRetryCount = value;
            }
        }

        public float ReleaseResourceInterval { get; set; }

        public bool UpdateIsEnabled
        {
            get
            {
                if (m_UpdateIsEnabled == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_UpdateIsEnabled.Value;
            }
            set
            {
                if (m_UpdateIsEnabled != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_UpdateIsEnabled = value;
            }
        }

        public string UpdateRelativePathFormat
        {
            get
            {
                if (m_UpdateRelativePathFormat == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_UpdateRelativePathFormat;
            }

            set
            {
                if (m_UpdateRelativePathFormat != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_UpdateRelativePathFormat = value ?? throw new ArgumentException("Shouldn't be null.", nameof(value));
            }
        }

        /// <inheritdoc />
        public string BundleVersion
        {
            get
            {
                if (string.IsNullOrEmpty(m_BundleVersion))
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_BundleVersion;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Shouldn't be null or empty.", nameof(value));
                }

                if (!string.IsNullOrEmpty(m_BundleVersion))
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_BundleVersion = value;
            }
        }

        public string ReadWritePath
        {
            get
            {
                if (string.IsNullOrEmpty(m_ReadWritePath))
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_ReadWritePath;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Shouldn't be null or empty.", nameof(value));
                }

                if (!string.IsNullOrEmpty(m_ReadWritePath))
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_ReadWritePath = value;
            }
        }

        public string InstallerPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_InstallerPath))
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_InstallerPath;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Shouldn't be null or empty.", nameof(value));
                }

                if (!string.IsNullOrEmpty(m_InstallerPath))
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_InstallerPath = value;
            }
        }

        public string RunningPlatform
        {
            get
            {
                if (string.IsNullOrEmpty(m_RunningPlatform))
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_RunningPlatform;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Shouldn't be null or empty.", nameof(value));
                }

                if (!string.IsNullOrEmpty(m_RunningPlatform))
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_RunningPlatform = value;
            }
        }

        private string InstallerIndexPath
        {
            get
            {
                if (m_InstallerIndexPath == null)
                {
                    m_InstallerIndexPath = Path.Combine(InstallerPath, Constant.IndexFileName);
                }

                return m_InstallerIndexPath;
            }
        }

        private string ReadWriteIndexPath
        {
            get
            {
                if (m_ReadWriteIndexPath == null)
                {
                    m_ReadWriteIndexPath = Path.Combine(ReadWritePath, Constant.IndexFileName);
                }

                return m_ReadWriteIndexPath;
            }
        }

        private string CachedRemoteIndexPath
        {
            get
            {
                if (m_CachedRemoteIndexPath == null)
                {
                    m_CachedRemoteIndexPath = Path.Combine(ReadWritePath, Constant.CachedRemoteIndexFileName);
                }

                return m_CachedRemoteIndexPath;
            }
        }

        public int AssetCachePoolCapacity
        {
            get
            {
                if (m_AssetCachePoolCapacity == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_AssetCachePoolCapacity.Value;
            }

            set
            {
                if (m_AssetCachePoolCapacity != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be greater than 0.");
                }

                m_AssetCachePoolCapacity = value;
            }
        }

        public int ResourceCachePoolCapacity
        {
            get
            {
                if (m_ResourceCachePoolCapacity == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_ResourceCachePoolCapacity.Value;
            }

            set
            {
                if (m_ResourceCachePoolCapacity != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be greater than 0.");
                }

                m_ResourceCachePoolCapacity = value;
            }
        }

        public int AssetAccessorPoolCapacity
        {
            get
            {
                if (m_AssetAccessorPoolCapacity == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_AssetAccessorPoolCapacity.Value;
            }

            set
            {
                if (m_AssetAccessorPoolCapacity != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be greater than 0.");
                }

                m_AssetAccessorPoolCapacity = value;
            }
        }

        /// <inheritdoc />
        public int UpdateSizeBeforeSavingReadWriteIndex
        {
            get
            {
                if (m_UpdateSizeBeforeSavingReadWriteIndex == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_UpdateSizeBeforeSavingReadWriteIndex.Value;
            }

            set
            {
                if (m_UpdateSizeBeforeSavingReadWriteIndex != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be non-negative.");
                }

                m_UpdateSizeBeforeSavingReadWriteIndex = value;
            }
        }

        public IAssetServiceConfigReader ConfigReader { get; }

        /// <inheritdoc />
        public IResourceUpdater ResourceUpdater => m_Updater;

        public AssetService(
            IAssetServiceConfigReader configReader,
            IDownloadService downloadService,
            IAssetIndexForInstallerLoader assetIndexForInstallerLoader,
            ISimpleFactory<IAssetLoadingTaskImpl> assetLoadingTaskImplFactory,
            ISimpleFactory<IResourceLoadingTaskImpl> resourceLoadingTaskImplFactory,
            IObjectDestroyer<object> resourceDestroyer,
            IRefPoolService refPoolService,
            ITickService tickService) : base(tickService)
        {
            m_DownloadService = downloadService;
            m_AssetIndexForInstallerLoader = assetIndexForInstallerLoader;
            m_AssetLoadingTaskImplFactory = assetLoadingTaskImplFactory;
            m_ResourceLoadingTaskImplFactory = resourceLoadingTaskImplFactory;
            m_ResourceDestroyer = resourceDestroyer;
            m_RefPoolService = refPoolService;
            ConfigReader = configReader;
            ApplyConfig();
            m_Preparer = new Preparer(this);
            m_UpdateChecker = new UpdateChecker(this);
            m_Updater = new Updater(this);
            m_Loader = new Loader(this);
        }

        private void ApplyConfig()
        {
            RunningPlatform = ConfigReader.RunningPlatform;
            UpdateIsEnabled = ConfigReader.UpdateIsEnabled;
            DownloadRetryCount = ConfigReader.DownloadRetryCount;
            ConcurrentAssetLoaderCount = ConfigReader.ConcurrentAssetLoaderCount;
            ConcurrentResourceLoaderCount = ConfigReader.ConcurrentResourceLoaderCount;
            AssetCachePoolCapacity = ConfigReader.AssetCachePoolCapacity;
            ResourceCachePoolCapacity = ConfigReader.ResourceCachePoolCapacity;
            AssetAccessorPoolCapacity = ConfigReader.AssetAccessorPoolCapacity;
            UpdateRelativePathFormat = ConfigReader.UpdateRelativePathFormat;
            ReadWritePath = ConfigReader.ReadWritePath;
            InstallerPath = ConfigReader.InstallerPath;
            ReleaseResourceInterval = ConfigReader.ReleaseResourceInterval;
            UpdateSizeBeforeSavingReadWriteIndex = ConfigReader.UpdateSizeBeforeSavingReadWriteIndex;

            foreach (var urlStr in ConfigReader.UpdateServerRootUrls)
            {
                AddUpdateServerRootUrl(urlStr);
            }
        }


        protected override void OnUpdate(TimeStruct timeStruct)
        {
            m_Loader.Update(timeStruct);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsLoadingAnyAsset)
                {
                    InternalLog.Warning("Some asset is still being loaded.");
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public void AddUpdateServerRootUrl(string updateServerRootUrl)
        {
            m_UpdateServerRootUrls.Add(new Uri(updateServerRootUrl, UriKind.Absolute));
        }

        /// <inheritdoc />
        public void Prepare(AssetServicePrepareCallbackSet callbackSet, object context)
        {
            if (m_Preparer.Status != PreparerStatus.None)
            {
                throw new InvalidOperationException("Preparation already kicked off.");
            }

            m_Preparer.Run(callbackSet, context);
        }

        /// <inheritdoc />
        public void CheckUpdate(AssetIndexRemoteFileInfo remoteIndexFileInfo, UpdateCheckCallbackSet callbackSet, object context)
        {
            if (m_Preparer.Status != PreparerStatus.Success)
            {
                throw new InvalidOperationException("Preparation not successfully done.");
            }

            m_UpdateChecker.Run(remoteIndexFileInfo, callbackSet, context);
        }

        /// <inheritdoc />
        public IAssetAccessor LoadAsset(string assetPath, LoadAssetCallbackSet callbackSet, object context)
        {
            if (m_UpdateChecker.Status != UpdateCheckerStatus.Success)
            {
                throw new InvalidOperationException("Update checking not successfully done.");
            }

            return m_Loader.LoadAsset(assetPath, false, callbackSet, context);
        }

        /// <inheritdoc />
        public int GetAssetResourceGroupId(string assetPath)
        {
            if (m_UpdateChecker.Status != UpdateCheckerStatus.Success)
            {
                throw new InvalidOperationException("Update checking not successfully done.");
            }

            return m_Loader.GetAssetResourceGroupId(assetPath);
        }

        /// <inheritdoc />
        public IAssetAccessor LoadSceneAsset(string sceneAssetPath, LoadAssetCallbackSet callbackSet, object context)
        {
            if (m_UpdateChecker.Status != UpdateCheckerStatus.Success)
            {
                throw new InvalidOperationException("Update checking not successfully done.");
            }

            return m_Loader.LoadAsset(sceneAssetPath, true, callbackSet, context);
        }

        /// <inheritdoc />
        public void UnloadAsset(IAssetAccessor assetAccessor)
        {
            if (m_UpdateChecker.Status != UpdateCheckerStatus.Success)
            {
                throw new InvalidOperationException("Update checking not successfully done.");
            }

            m_Loader.UnloadAsset((AssetAccessor)assetAccessor);
        }

        /// <inheritdoc />
        public bool IsLoadingAnyAsset
        {
            get
            {
                if (m_UpdateChecker.Status != UpdateCheckerStatus.Success)
                {
                    return false;
                }

                return m_Loader.IsLoadingAnyAsset;
            }
        }

        /// <inheritdoc />
        public void RequestUnloadUnusedResources()
        {
            if (m_UpdateChecker.Status != UpdateCheckerStatus.Success)
            {
                return;
            }

            m_Loader.RequestUnloadUnusedResources();
        }

        private void SaveReadWriteIndex()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReadWriteIndexPath) ?? throw new NullReferenceException());
            using (var fs = File.OpenWrite(ReadWriteIndexPath))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    new AssetIndexSerializerV2().ToBinary(bw, m_ReadWriteIndex);
                }
            }
        }

        private void CleanUpReadWritePath()
        {
            if (!Directory.Exists(ReadWritePath))
            {
                return;
            }

            foreach (var resourceInfo in m_ReadWriteIndex.ResourceInfos.Values)
            {
                var filePath = Path.Combine(ReadWritePath, resourceInfo.Path);
                File.Delete(filePath);
            }

            foreach (var f in Directory.GetFiles(ReadWritePath))
            {
                File.Delete(f);
            }

            foreach (var d in Directory.GetDirectories(ReadWritePath))
            {
                Directory.Delete(d, true);
            }
        }

        private bool TryCleanUpReadWritePathOrFail(Action<Exception, string> failFunc)
        {
            try
            {
                CleanUpReadWritePath();
            }
            catch (IOException e)
            {
                failFunc(e, "Cannot clean up read-write path. Inner exception is '{0}'.");
                return false;
            }

            return true;
        }

        private bool TrySaveReadWriteIndexOrFail(Action<Exception, string> failFunc)
        {
            try
            {
                SaveReadWriteIndex();
            }
            catch (Exception e)
            {
                failFunc(e, "Cannot write to read-write index file. Inner exception is '{0}'.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get all asset cache queries.
        /// </summary>
        /// <returns></returns>
        /// <remarks>For debug use.</remarks>
        public IDictionary<string, AssetCacheQuery> GetAssetCacheQueries()
        {
            return m_Loader == null ? new Dictionary<string, AssetCacheQuery>() : m_Loader.GetAssetCacheQueries();
        }

        /// <summary>
        /// Get all resource cache queries.
        /// </summary>
        /// <returns></returns>
        /// <remarks>For debug use.</remarks>
        public IDictionary<string, ResourceCacheQuery> GetResourceCacheQueries()
        {
            return m_Loader == null ? new Dictionary<string, ResourceCacheQuery>() : m_Loader.GetResourceCacheQueries();
        }

        private static void DeserializeAssetIndex(BinaryReader br, AssetIndexBase assetIndex)
        {
            var streamPosition = br.BaseStream.Position;
            var header = br.ReadString();
            IBinarySerializer<AssetIndexBase> serializer;
            if (header == assetIndex.ObsoleteHeader)
            {
                serializer = new AssetIndexSerializer();
            }
            else if (header == assetIndex.Header)
            {
                var version = br.ReadInt16();

                if (version == 2)
                {
                    serializer = new AssetIndexSerializerV2();
                }
                else
                {
                    throw new InvalidOperationException($"Version {version} of {assetIndex.GetType()} is not supported.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Header {header} of {assetIndex.GetType()} is not supported.");
            }

            InternalLog.Debug($"DeserializeAssetIndex. Type: {assetIndex.GetType()}, Serializer type: {serializer.GetType()}.");
            // TODO: How do I avoid seeking back to the beginning of the stream. Do I need specific header serializer classes?
            br.BaseStream.Seek(streamPosition, SeekOrigin.Begin);
            serializer.FromBinary(br, assetIndex);
        }
    }
}
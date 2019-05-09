using System;
using System.Collections.Generic;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public sealed partial class AssetModule : BaseModule, IAssetModule
    {
        private IDownloadModule m_DownloadModule = null;
        private IRefPoolModule m_RefPoolModule = null;
        private ISimpleFactory<IAssetLoadingTaskImpl> m_AssetLoadingTaskImplFactory = null;
        private ISimpleFactory<IResourceLoadingTaskImpl> m_ResourceLoadingTaskImplFactory = null;
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

        private IAssetIndexForInstallerLoader m_AssetIndexForInstallerLoader = null;
        private IObjectDestroyer<object> m_ResourceDestroyer = null;

        private readonly AssetIndexForInstaller m_InstallerIndex = new AssetIndexForInstaller();
        private readonly AssetIndexForReadWrite m_ReadWriteIndex = new AssetIndexForReadWrite();
        private readonly AssetIndexForRemote m_RemoteIndex = new AssetIndexForRemote();

        private Preparer m_Preparer = null;
        private UpdateChecker m_UpdateChecker = null;
        private Updater m_Updater = null;
        private Loader m_Loader = null;

        // Initialized by the update checker and used by the updater.
        private readonly Dictionary<int, ResourceGroupUpdateSummary> ResourceGroupUpdateSummaries =
            new Dictionary<int, ResourceGroupUpdateSummary>();

        /// <inheritdoc />
        public IDownloadModule DownloadModule
        {
            get { return m_DownloadModule ?? throw new InvalidOperationException("Not set."); }

            set
            {
                if (m_DownloadModule != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_DownloadModule = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <inheritdoc />
        public IRefPoolModule RefPoolModule
        {
            get { return m_RefPoolModule ?? throw new InvalidOperationException("Not set."); }

            set
            {
                if (m_RefPoolModule != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_RefPoolModule = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <inheritdoc />
        public ISimpleFactory<IAssetLoadingTaskImpl> AssetLoadingTaskImplFactory
        {
            get { return m_AssetLoadingTaskImplFactory ?? throw new InvalidOperationException("Not set."); }

            set
            {
                if (m_AssetLoadingTaskImplFactory != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_AssetLoadingTaskImplFactory = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <inheritdoc />
        public ISimpleFactory<IResourceLoadingTaskImpl> ResourceLoadingTaskImplFactory
        {
            get { return m_ResourceLoadingTaskImplFactory ?? throw new InvalidOperationException("Not set."); }

            set
            {
                if (m_ResourceLoadingTaskImplFactory != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_ResourceLoadingTaskImplFactory = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public float ReleaseResourceInterval { get; set; }

        /// <inheritdoc />
        public IAssetIndexForInstallerLoader IndexForInstallerLoader
        {
            get
            {
                if (m_AssetIndexForInstallerLoader == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_AssetIndexForInstallerLoader;
            }
            set
            {
                if (m_AssetIndexForInstallerLoader != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_AssetIndexForInstallerLoader = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <inheritdoc />
        public IObjectDestroyer<object> ResourceDestroyer
        {
            get
            {
                if (m_ResourceDestroyer == null)
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_ResourceDestroyer;
            }
            set
            {
                if (m_ResourceDestroyer != null)
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_ResourceDestroyer = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public string UpdateRelativePathFormat
        {
            get
            {
                if (string.IsNullOrEmpty(m_UpdateRelativePathFormat))
                {
                    throw new InvalidOperationException("Not set.");
                }

                return m_UpdateRelativePathFormat;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Shouldn't be null or empty.", nameof(value));
                }

                if (!string.IsNullOrEmpty(m_UpdateRelativePathFormat))
                {
                    throw new InvalidOperationException("Already set.");
                }

                m_UpdateRelativePathFormat = value;
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IResourceUpdater ResourceUpdater => m_Updater;

        /// <inheritdoc />
        public override void Init()
        {
            base.Init();
            m_Preparer = new Preparer(this);
            m_UpdateChecker = new UpdateChecker(this);
            m_Updater = new Updater(this);
            m_Loader = new Loader(this);
        }

        /// <inheritdoc />
        public override void Update(TimeStruct timeStruct)
        {
            base.Update(timeStruct);
            m_Loader.Update(timeStruct);
        }

        /// <inheritdoc />
        public void AddUpdateServerRootUrl(string updateServerRootUrl)
        {
            m_UpdateServerRootUrls.Add(new Uri(updateServerRootUrl, UriKind.Absolute));
        }

        /// <inheritdoc />
        public void Prepare(AssetModulePrepareCallbackSet callbackSet, object context)
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
                    m_ReadWriteIndex.ToBinary(bw);
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

        /// <inheritdoc />
        public IDictionary<string, AssetCacheQuery> GetAssetCacheQueries()
        {
            return m_Loader == null ? new Dictionary<string, AssetCacheQuery>() : m_Loader.GetAssetCacheQueries();
        }

        /// <inheritdoc />
        public IDictionary<string, ResourceCacheQuery> GetResourceCacheQueries()
        {
            return m_Loader == null ? new Dictionary<string, ResourceCacheQuery>() : m_Loader.GetResourceCacheQueries();
        }
    }
}
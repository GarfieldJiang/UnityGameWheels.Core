namespace COL.UnityGameWheels.Core.Asset
{
    using System;
    using System.IO;

    public partial class AssetModule
    {
        private class Preparer
        {
            public PreparerStatus Status { get; private set; }

            private readonly AssetModule m_Owner = null;
            private object m_Context = null;
            private AssetModulePrepareCallbackSet m_CallbackSet;

            private IAssetIndexForInstallerLoader IndexForInstallerLoader => m_Owner.IndexForInstallerLoader;

            private AssetIndexForInstaller InstallerIndex => m_Owner.m_InstallerIndex;

            private AssetIndexForReadWrite ReadWriteIndex => m_Owner.m_ReadWriteIndex;

            public Preparer(AssetModule owner)
            {
                m_Owner = owner;
                Status = PreparerStatus.None;
            }

            public void Run(AssetModulePrepareCallbackSet callbackSet, object context)
            {
                if (Status != PreparerStatus.None)
                {
                    throw new InvalidOperationException("Preparation already run.");
                }

                if (IndexForInstallerLoader == null)
                {
                    throw new InvalidOperationException("Read-only index loader is not set.");
                }

                m_CallbackSet = callbackSet;
                m_Context = context;
                Status = PreparerStatus.Running;

                IndexForInstallerLoader.Load(m_Owner.InstallerIndexPath, new LoadAssetIndexForInstallerCallbackSet
                {
                    OnFailure = OnLoadInstallerIndexFailure,
                    OnSuccess = OnLoadInstallerIndexSuccess,
                }, null);
            }

            private void OnLoadInstallerIndexFailure(object context)
            {
                Status = PreparerStatus.None;
                var errorMessage = "Cannot load the index file in the installer path.";
                if (m_CallbackSet.OnFailure != null)
                {
                    m_CallbackSet.OnFailure(errorMessage, m_Context);
                }
                else
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }

            private void OnLoadInstallerIndexSuccess(Stream stream, object context)
            {
                if (!TryParseInstallerIndexOrFail(stream))
                {
                    return;
                }

                if (!TryParseReadWriteIndexOrFail())
                {
                    return;
                }

                Succeed();
            }

            private void Succeed()
            {
                Status = PreparerStatus.Success;
                m_CallbackSet.OnSuccess?.Invoke(m_Context);
            }

            private void Fail(Exception e, string errorMessageFormat)
            {
                Status = PreparerStatus.None;
                if (m_CallbackSet.OnFailure == null)
                {
                    throw new Exception(string.Empty, e);
                }

                m_CallbackSet.OnFailure(
                    Utility.Text.Format(errorMessageFormat, e), m_Context);
            }

            private bool TryParseInstallerIndexOrFail(Stream stream)
            {
                try
                {
                    using (var br = new BinaryReader(stream))
                    {
                        InstallerIndex.FromBinary(br);
                    }
                }
                catch (Exception e)
                {
                    Fail(e, "Cannot parse the index file in the installer path. Inner exception is '{0}'.");
                    return false;
                }

                return true;
            }

            private bool TryParseReadWriteIndexOrFail()
            {
                // If the read-write index doesn't exist. Just keep 'ReadWriteIndex' empty as it is.
                if (!File.Exists(m_Owner.ReadWriteIndexPath))
                {
                    return true;
                }

                try
                {
                    using (var stream = File.OpenRead(m_Owner.ReadWriteIndexPath))
                    {
                        using (var br = new BinaryReader(stream))
                        {
                            ReadWriteIndex.FromBinary(br);
                        }
                    }
                }
                catch (Exception e)
                {
                    CoreLog.Warning($"Cannot parse the index file with exception '{e}'. Will try to clean up read-write path.");
                    ReadWriteIndex.Clear();
                    return !File.Exists(m_Owner.ReadWriteIndexPath) || m_Owner.TryCleanUpReadWritePathOrFail(Fail);
                }

                return true;
            }
        }
    }
}
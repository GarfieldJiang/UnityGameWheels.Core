using System;

namespace COL.UnityGameWheels.Core.Asset
{
    public class AssetIndexWrongVersionException : Exception
    {
        public short ExpectedVersion { get; }
        public short ActualVersion { get; }

        public AssetIndexWrongVersionException(short expectedVersion, short actualVersion)
            : base($"Wrong version. Expected: {expectedVersion}, Actual: {actualVersion}.")
        {
            ExpectedVersion = expectedVersion;
            ActualVersion = actualVersion;
        }
    }
}
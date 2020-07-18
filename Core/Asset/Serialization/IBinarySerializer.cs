using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public interface IBinarySerializer<T>
    {
        void ToBinary(BinaryWriter bw, T obj);

        void FromBinary(BinaryReader br, T obj);
    }
}
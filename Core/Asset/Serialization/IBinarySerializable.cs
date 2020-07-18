using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Interface to describe something serializable to binary.
    /// </summary>
    public interface IBinarySerializable
    {
        /// <summary>
        /// Writes to binary.
        /// </summary>
        /// <param name="bw">Binary writer.</param>
        void ToBinary(BinaryWriter bw);

        /// <summary>
        /// Reads from binary.
        /// </summary>
        /// <param name="br">Reads from binary.</param>
        void FromBinary(BinaryReader br);
    }
}
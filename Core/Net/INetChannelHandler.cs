using System.IO;

namespace COL.UnityGameWheels.Core.Net
{
    public interface INetChannelHandler
    {
        void OnConnected();

        void OnError(string errorMessage, object errorData);

        void OnReceive(Packet packet);

        void Serialize(Packet packet, MemoryStream targetStream);

        ///  <summary>
        /// 
        ///  </summary>
        /// <param name="sourceStream"></param>
        ///  <remarks>Not called on the main thread.</remarks>
        IPacketHeader DeserializePacketHeader(MemoryStream sourceStream);

        /// <summary>
        ///
        /// </summary>
        /// <remarks>Not called on the main thread.</remarks>
        Packet Deserialize(IPacketHeader packetHeader, MemoryStream sourceStream);

        /// <summary>
        ///
        /// </summary>
        /// <param name="packet"></param>
        /// <remarks>Not called on the main thread.</remarks>
        void OnRecycle(Packet packet);
    }
}
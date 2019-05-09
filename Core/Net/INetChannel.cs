using System;
using System.Net;

namespace COL.UnityGameWheels.Core.Net
{
    /// <summary>
    /// Interface to depict a single persistent network connection.
    /// </summary>
    public interface INetChannel : IDisposable
    {
        INetChannelHandler Handler { get; }

        string Name { get; }

        int ReceivePacketHeaderLength { get; }

        NetChannelState State { get; }

        IPEndPoint RemoteEndPoint { get; }

        IPEndPoint LocalEndPoint { get; }

        void Connect(IPAddress remoteIP, int port);

        void Close();

        void Send(Packet packet);

        void Update(TimeStruct timeStruct);
    }
}
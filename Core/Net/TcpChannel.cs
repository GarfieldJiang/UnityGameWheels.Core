using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace COL.UnityGameWheels.Core.Net
{
    public sealed class TcpChannel : INetChannel
    {
        private Socket m_Socket = null;
        private readonly SocketAsyncEventArgs m_ConnectionEventArgs = new SocketAsyncEventArgs();
        private readonly SocketAsyncEventArgs m_SendingEventArgs = new SocketAsyncEventArgs();
        private readonly SocketAsyncEventArgs m_ReceiveEventArgs = new SocketAsyncEventArgs();
        private readonly Queue<Packet> m_PacketsToSend = new Queue<Packet>();
        private readonly Queue<Packet> m_PacketsToReceive = new Queue<Packet>();
        private readonly List<Packet> m_PacketsToReceiveMainThread = new List<Packet>();
        private readonly List<Packet> m_PacketsToSendList = new List<Packet>();
        private readonly MemoryStream m_SendingStream = new MemoryStream(4096);
        private readonly byte[] m_ReceiveBuffer = new byte[4096];
        private readonly MemoryStream m_ReceiveStream = new MemoryStream(4096);
        private readonly List<ArraySegment<byte>> m_SendBufferList = new List<ArraySegment<byte>>(8);
        private int m_IsSending = 0;
        private IPacketHeader m_CurrentPacketHeader = null;
        private readonly object m_StateLock = new object();

        public INetChannelHandler Handler { get; private set; } = null;

        public int ReceivePacketHeaderLength { get; private set; } = 0;

        private NetChannelState m_State = NetChannelState.Disconnected;

        public NetChannelState State
        {
            get
            {
                lock (m_StateLock)
                {
                    return m_State;
                }
            }
        }

        public IPEndPoint RemoteEndPoint { get; private set; } = null;

        public IPEndPoint LocalEndPoint => (IPEndPoint)m_Socket?.LocalEndPoint;

        public string Name { get; private set; } = null;

        public TcpChannel(string name, INetChannelHandler handler, int receivePacketHeaderLength)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name is invalid.", nameof(name));
            }

            if (receivePacketHeaderLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(receivePacketHeaderLength), "Must be positive.");
            }

            Name = name;
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            ReceivePacketHeaderLength = receivePacketHeaderLength;
        }

        private void Clear()
        {
            var tmpList = new List<Packet>();
            lock (m_PacketsToSend)
            {
                tmpList.AddRange(m_PacketsToSend);
                m_PacketsToSend.Clear();
            }

            lock (m_PacketsToReceive)
            {
                tmpList.AddRange(m_PacketsToReceive);
                m_PacketsToReceive.Clear();
            }

            foreach (var packet in tmpList)
            {
                Handler.OnRecycle(packet);
            }
        }

        public void Send(Packet packet)
        {
            if (m_State != NetChannelState.Connected)
            {
                throw new InvalidOperationException(
                    $"Send() can be called only on state '{NetChannelState.Connected}'.");
            }

            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            lock (m_PacketsToSend)
            {
                m_PacketsToSend.Enqueue(packet);

                if (Interlocked.CompareExchange(ref m_IsSending, 1, 0) != 0)
                {
                    return;
                }

                CopyPacketsToSend();
            }

            DoSend();
        }

        public void Connect(IPAddress remoteIP, int port)
        {
            if (remoteIP == null)
            {
                throw new ArgumentNullException(nameof(remoteIP));
            }

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            NetChannelState? state = null;

            lock (m_StateLock)
            {
                if (m_State != NetChannelState.Disconnected)
                {
                    state = m_State;
                }
                else
                {
                    m_State = NetChannelState.Connecting;
                }
            }

            if (state != null)
            {
                Handler.OnError($"Cannot connect under state '{state}'.", null);
                return;
            }

            m_Socket = new Socket(remoteIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            RemoteEndPoint = new IPEndPoint(remoteIP, port);
            m_SendingEventArgs.Completed += OnSendCompleted;
            m_ReceiveEventArgs.Completed += OnReceiveCompleted;
            m_ReceiveEventArgs.SetBuffer(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length);
            m_ConnectionEventArgs.RemoteEndPoint = RemoteEndPoint;
            m_ConnectionEventArgs.Completed += OnConnectionComplete;

            m_Socket.ConnectAsync(m_ConnectionEventArgs);
        }

        public void Close()
        {
            lock (m_StateLock)
            {
                CoreLog.Debug("[TcpChannel OnClose] Start.");
                if (m_State == NetChannelState.Unknown || m_State == NetChannelState.Closed
                                                       || m_State == NetChannelState.Closing ||
                                                       m_State == NetChannelState.Disconnected)
                {
                    return;
                }

                m_State = NetChannelState.Closing;
            }

            Clear();
            Interlocked.Exchange(ref m_IsSending, 0);

            m_SendingEventArgs.Completed -= OnSendCompleted;
            m_ReceiveEventArgs.Completed -= OnReceiveCompleted;
            m_ConnectionEventArgs.Completed -= OnConnectionComplete;
            m_ReceiveStream.Dispose();
            m_SendingStream.Dispose();

            CoreLog.Debug("[TcpChannel OnClose] Before socket shutdown and close.");
            try
            {
                m_Socket.Shutdown(SocketShutdown.Both);
                m_Socket.Close();
            }
            catch
            {
                // ignored
            }
            finally
            {
                CoreLog.Debug("[TcpChannel OnClose] finally block.");
                m_Socket = null;

                lock (m_StateLock)
                {
                    m_State = NetChannelState.Closed;
                }

                CoreLog.Debug("[TcpChannel OnClose] End.");
            }
        }

        public void Update(TimeStruct timeStruct)
        {
            lock (m_PacketsToReceive)
            {
                m_PacketsToReceiveMainThread.AddRange(m_PacketsToReceive);
                m_PacketsToReceive.Clear();
            }

            try
            {
                foreach (var packet in m_PacketsToReceiveMainThread)
                {
                    Handler.OnReceive(packet);
                }
            }
            finally
            {
                m_PacketsToReceiveMainThread.Clear();
            }
        }

        #region IDisposable Support

        private bool m_Disposed = false;

        private void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            if (disposing)
            {
                Close();
            }

            m_Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        private void CopyPacketsToSend()
        {
            m_PacketsToSendList.Clear();
            m_PacketsToSendList.AddRange(m_PacketsToSend);
            m_PacketsToSend.Clear();
        }

        private void OnConnectionComplete(object sender, SocketAsyncEventArgs e)
        {
            // Connection failed.
            if (e.SocketError != SocketError.Success)
            {
                Close();
                OnError("Connection failed. Error data is a SocketError.", e.SocketError);
                return;
            }

            // Connection succeeded. Start receiving things.
            lock (m_StateLock)
            {
                m_State = NetChannelState.Connected;
            }

            Handler.OnConnected();
            Receive();
        }

        private void DoSend()
        {
            m_SendingStream.Position = 0L;

            m_SendBufferList.Clear();
            foreach (var packet in m_PacketsToSendList)
            {
                Handler.Serialize(packet, m_SendingStream);
            }

            m_SendBufferList.Add(new ArraySegment<byte>(m_SendingStream.GetBuffer(), 0,
                (int)m_SendingStream.Position));
            m_SendingEventArgs.BufferList = m_SendBufferList;
            m_Socket.SendAsync(m_SendingEventArgs);
        }

        private void Receive()
        {
            CoreLog.Debug("[TcpChannel Receive]");
            m_Socket.ReceiveAsync(m_ReceiveEventArgs);
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            CoreLog.DebugFormat("[TcpChannel OnReceiveCompleted] socket error is '{0}', bytes transffered is {1}.",
                e.SocketError, e.BytesTransferred);

            // Receiving failed.
            if (e.SocketError != SocketError.Success)
            {
                Close();
                OnError("Receiving data failed. Error data is a SocketError.", e.SocketError);
                return;
            }

            // Server stops this connection.
            if (e.BytesTransferred == 0)
            {
                Close();
                OnError("Server stops this connection.", null);
                return;
            }

            // Receiving succeeded.
            m_ReceiveStream.Write(m_ReceiveBuffer, 0, e.BytesTransferred);
            long length = m_ReceiveStream.Position;
            m_ReceiveStream.Position = 0;

            while (m_ReceiveStream.Position < length)
            {
                if (m_CurrentPacketHeader == null) // Should read the next packet header.
                {
                    // Packet header has not been completed received.
                    if (length - m_ReceiveStream.Position < ReceivePacketHeaderLength)
                    {
                        break;
                    }

                    // Read the packet header.
                    m_CurrentPacketHeader = Handler.DeserializePacketHeader(m_ReceiveStream);
                }
                else // Should read the next packet body.
                {
                    // Current packet has not been completely received.
                    if (length - m_ReceiveStream.Position < m_CurrentPacketHeader.PacketLength)
                    {
                        break;
                    }

                    {
                        var packetHeader = m_CurrentPacketHeader;
                        m_CurrentPacketHeader = null;
                        var packet = Handler.Deserialize(packetHeader, m_ReceiveStream);
                        lock (m_PacketsToReceive)
                        {
                            m_PacketsToReceive.Enqueue(packet);
                        }
                    }
                }
            }

            var underlyingBuffer = m_ReceiveStream.GetBuffer();
            Buffer.BlockCopy(underlyingBuffer, (int)m_ReceiveStream.Position, underlyingBuffer, 0,
                (int)(length - m_ReceiveStream.Position));
            m_ReceiveStream.Position = length - m_ReceiveStream.Position;

            if (m_State != NetChannelState.Connected)
            {
                return;
            }

            Receive();
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            foreach (var packet in m_PacketsToSendList)
            {
                Handler.OnRecycle(packet);
            }

            // Sending failed.
            if (e.SocketError != SocketError.Success)
            {
                CoreLog.DebugFormat("[TcpChannel OnSendCompleted] Failure, SocketError={0}.", e.SocketError);
                Close();
                OnError("Sending data failed. Error data is a SocketError.", e.SocketError);
                return;
            }

            CoreLog.DebugFormat("[TcpChannel OnSendCompleted] Success, bytesTransferred={0}.", e.BytesTransferred);
            lock (m_PacketsToSend)
            {
                if (m_PacketsToSend.Count <= 0)
                {
                    Interlocked.Exchange(ref m_IsSending, 0);
                    return;
                }

                CopyPacketsToSend();
            }

            DoSend();
        }

        private void OnError(string errorMessage, SocketError? socketError)
        {
            CoreLog.Debug("[TcpChannel OnError] Start.");
            try
            {
                Handler.OnError(errorMessage, socketError == null ? null : (object)socketError.Value);
            }
            catch
            {
                // ignored
            }
            finally
            {
                CoreLog.Debug("[TcpChannel OnError] End.");
            }
        }
    }
}
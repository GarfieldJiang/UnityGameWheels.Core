namespace COL.UnityGameWheels.Core.Net
{
    public class DefaultNetChannelFactory : INetChannelFactory
    {
        public INetChannel Create(string name, string typeKey, INetChannelHandler handler, int receivePacketHeaderLength)
        {
            return new TcpChannel(name, handler, receivePacketHeaderLength);
        }
    }
}

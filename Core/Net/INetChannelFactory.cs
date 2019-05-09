namespace COL.UnityGameWheels.Core.Net
{
    public interface INetChannelFactory
    {
        INetChannel Create(string name, string typeKey, INetChannelHandler handler, int receivePacketHeaderLength);
    }
}
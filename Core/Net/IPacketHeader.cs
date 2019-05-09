namespace COL.UnityGameWheels.Core.Net
{
    public interface IPacketHeader
    {
        int PacketLength { get; set; }

        int PacketId { get; set; }
    }
}
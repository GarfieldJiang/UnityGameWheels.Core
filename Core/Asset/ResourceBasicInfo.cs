using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    public class ResourceBasicInfo : IBinarySerializable
    {
        public string Path = string.Empty;

        public int GroupId = 0;

        public void FromBinary(BinaryReader br)
        {
            Path = br.ReadString();
            GroupId = br.ReadInt32();
        }

        public void ToBinary(BinaryWriter bw)
        {
            bw.Write(Path);
            bw.Write(GroupId);
        }
    }
}
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    internal class ResourceBasicInfoSerializer : IBinarySerializer<ResourceBasicInfo>
    {
        public void ToBinary(BinaryWriter bw, ResourceBasicInfo obj)
        {
            bw.Write(obj.Path);
            bw.Write(obj.GroupId);
            bw.Write(obj.DependingResourcePaths.Count);
            foreach (var dependingResourcePath in obj.DependingResourcePaths)
            {
                bw.Write(dependingResourcePath);
            }
        }

        public void FromBinary(BinaryReader br, ResourceBasicInfo obj)
        {
            obj.Path = br.ReadString();
            obj.GroupId = br.ReadInt32();
            obj.DependingResourcePaths.Clear();
            var dependingResourcePathCount = br.ReadInt32();
            for (int i = 0; i < dependingResourcePathCount; i++)
            {
                obj.DependingResourcePaths.Add(br.ReadString());
            }
        }
    }
}
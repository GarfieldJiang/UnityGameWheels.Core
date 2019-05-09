using System.Collections.Generic;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Asset info.
    /// </summary>
    public class AssetInfo : IBinarySerializable
    {
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the resource path.
        /// </summary>
        /// <value>The resource path.</value>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Gets the dependency asset paths.
        /// </summary>
        /// <value>The dependency asset paths.</value>
        public HashSet<string> DependencyAssetPaths { get; } = new HashSet<string>();

        /// <summary>
        /// Writes to binary.
        /// </summary>
        /// <param name="bw">Binary writer.</param>
        public void ToBinary(BinaryWriter bw)
        {
            bw.Write(Path);
            bw.Write(ResourcePath);
            bw.Write(DependencyAssetPaths.Count);
            foreach (var path in DependencyAssetPaths)
            {
                bw.Write(path);
            }
        }

        /// <summary>
        /// Reads from binary.
        /// </summary>
        /// <param name="br">Binary reader.</param>
        public void FromBinary(BinaryReader br)
        {
            Path = br.ReadString();
            ResourcePath = br.ReadString();

            DependencyAssetPaths.Clear();
            int dependencyAssetPathCount = br.ReadInt32();
            for (int i = 0; i < dependencyAssetPathCount; i++)
            {
                DependencyAssetPaths.Add(br.ReadString());
            }
        }
    }
}
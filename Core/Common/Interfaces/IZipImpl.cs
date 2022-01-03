using System.IO;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Interface to decompress a zip file.
    /// </summary>
    public interface IZipImpl
    {
        /// <summary>
        /// Decompress a zip stream.
        /// </summary>
        /// <param name="archiveStream">The zip compressed stream.</param>
        /// <param name="dstStream">Where the data should be decompressed.</param>
        void Unzip(Stream archiveStream, Stream dstStream);

        /// <summary>
        /// Compress a zip stream.
        /// </summary>
        /// <param name="srcStream">The source data.</param>
        /// <param name="archiveStream">The zip compressed stream.</param>
        void Zip(Stream srcStream, Stream archiveStream);
    }
}
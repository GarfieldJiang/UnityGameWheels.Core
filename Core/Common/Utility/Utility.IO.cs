using System.IO;

namespace COL.UnityGameWheels.Core
{
    public static partial class Utility
    {
        /// <summary>
        /// IO utility.
        /// </summary>
        public static class IO
        {
            /// <summary>
            /// Remove empty folders in post-order.
            /// </summary>
            /// <param name="startingDirectoryPath">Where the operation starts.</param>
            public static void DeleteEmptyFolders(string startingDirectoryPath)
            {
                DeleteEmptyFolders(new DirectoryInfo(startingDirectoryPath));
            }

            /// <summary>
            /// Remove empty folders in post-order.
            /// </summary>
            /// <param name="startingDirectory">Where the operation starts.</param>
            public static void DeleteEmptyFolders(DirectoryInfo startingDirectory)
            {
                if (!startingDirectory.Exists)
                {
                    throw new DirectoryNotFoundException(Text.Format("Directory doesn't exist at path '{0}'.", startingDirectory.FullName));
                }

                DeleteEmptyFoldersInternal(startingDirectory);
            }

            private static void DeleteEmptyFoldersInternal(DirectoryInfo startingDirectory)
            {
                foreach (var dir in startingDirectory.GetDirectories())
                {
                    DeleteEmptyFoldersInternal(dir);
                    if (dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0)
                    {
                        dir.Delete(false);
                    }
                }
            }
        }
    }
}
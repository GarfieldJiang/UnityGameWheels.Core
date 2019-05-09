using System;
using System.Text;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// String builder cache.
    /// </summary>
    /// <remarks>https://referencesource.microsoft.com</remarks>
    public class StringBuilderCache
    {
        // The value 360 was chosen in discussion with performance experts as a compromise between using
        // as litle memory (per thread) as possible and still covering a large part of short-lived
        // StringBuilder creations on the startup path of VS designers.
        private const int MAX_BUILDER_SIZE = 360;

        [ThreadStatic]
        private static StringBuilder CachedInstance;

        /// <summary>
        /// Acquire a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="capacity">Desired capacity.</param>
        /// <returns>The StringBuilder object.</returns>
        public static StringBuilder Acquire(int capacity = 0x100)
        {
            if (capacity <= MAX_BUILDER_SIZE)
            {
                StringBuilder sb = CachedInstance;
                if (sb != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= sb.Capacity)
                    {
                        CachedInstance = null;
                        sb.Length = 0;
                        return sb;
                    }
                }
            }
            return new StringBuilder(capacity);
        }

        /// <summary>
        /// Release a <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="sb">The StringBuilder object to recycle.</param>
        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity <= MAX_BUILDER_SIZE)
            {
                CachedInstance = sb;
            }
        }

        /// <summary>
        /// Get the string value of the <see cref="StringBuilder"/> object and release it.
        /// </summary>
        /// <param name="sb">The StringBuilder object to recycle.</param>
        /// <returns>String value.</returns>
        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            Release(sb);
            return result;
        }
    }
}
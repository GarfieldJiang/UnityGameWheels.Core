using System;

namespace COL.UnityGameWheels.Core
{
    public static partial class Utility
    {
        /// <summary>
        /// Buffer utility.
        /// </summary>
        public static class Buffer
        {
            /// <summary>
            /// Compare equality of two buffers.
            /// </summary>
            /// <param name="bufferA">First buffer.</param>
            /// <param name="offsetA">Start index of the first buffer.</param>
            /// <param name="bufferB">Second buffer.</param>
            /// <param name="offsetB">Start index of the second buffer.</param>
            /// <param name="length">Length in bytes of the comparison.</param>
            /// <returns>Whether the two buffers are equal.</returns>
            public static bool BufferCompare(byte[] bufferA, int offsetA, byte[] bufferB, int offsetB, int length)
            {
                if (bufferA == null)
                {
                    throw new ArgumentNullException("bufferA");
                }

                if (bufferB == null)
                {
                    throw new ArgumentNullException("bufferB");
                }

                if (length < 0)
                {
                    throw new ArgumentOutOfRangeException("length", "Must be non-negative.");
                }

                if (offsetA < 0 || offsetA + length > bufferA.Length)
                {
                    throw new ArgumentOutOfRangeException("offsetA");
                }

                if (offsetB < 0 || offsetB + length > bufferB.Length)
                {
                    throw new ArgumentOutOfRangeException("offsetB");
                }

                for (int i = 0; i < length; i++)
                {
                    if (bufferA[offsetA + i] != bufferB[offsetB + i])
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Compare equality of two buffers.
            /// </summary>
            /// <param name="bufferA">First buffer.</param>
            /// <param name="bufferB">Second buffer.</param>
            /// <param name="offsetB">Start index of the second buffer.</param>
            /// <param name="lengthB">Length in bytes of the comparison.</param>
            /// <returns>Whether the two buffers are equal.</returns>s
            public static bool BufferCompare(byte[] bufferA, byte[] bufferB, int offsetB, int lengthB)
            {
                if (bufferA == null)
                {
                    throw new ArgumentNullException("bufferA");
                }

                if (bufferB == null)
                {
                    throw new ArgumentNullException("bufferB");
                }

                if (lengthB < 0)
                {
                    throw new ArgumentOutOfRangeException("lengthB", "Must be non-negative.");
                }

                if (offsetB < 0 || offsetB + lengthB > bufferB.Length)
                {
                    throw new ArgumentOutOfRangeException("offsetB");
                }

                if (bufferA.Length != lengthB)
                {
                    return false;
                }

                for (int i = 0; i < bufferA.Length; i++)
                {
                    if (bufferA[i] != bufferB[i + offsetB])
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Get hex string representation of a given buffer.
            /// </summary>
            /// <param name="capitalLetters">Whether to use capital letters for A-F.</param>
            /// <param name="buffer">Buffer.</param>
            /// <returns>The hex string.</returns>s
            public static string ToHexString(bool capitalLetters, byte[] buffer)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }

                return ToHexString(capitalLetters, buffer, 0, buffer.Length);
            }

            /// <summary>
            /// Get hex string representation of a given buffer.
            /// </summary>
            /// <param name="capitalLetters">Whether to use capital letters for A-F.</param>
            /// <param name="buffer">Buffer.</param>
            /// <param name="offset">Start index.</param>
            /// <param name="length">Length.</param>
            /// <returns>The hex string.</returns>
            public static string ToHexString(bool capitalLetters, byte[] buffer, int offset, int length)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }

                if (length < 0)
                {
                    throw new ArgumentOutOfRangeException("length", "Must be non-negative.");
                }

                var sb = StringBuilderCache.Acquire();
                for (int i = offset; i < offset + length; i++)
                {
                    sb.Append(buffer[i].ToString(capitalLetters ? "X2" : "x2"));
                }

                return StringBuilderCache.GetStringAndRelease(sb);
            }
        }
    }
}

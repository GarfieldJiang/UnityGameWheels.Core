using System;

namespace COL.UnityGameWheels.Core
{
    public static partial class Utility
    {
        /// <summary>
        /// Bit converter.
        /// </summary>
        public static class BitConverter
        {
            [ThreadStatic]
            private static byte[] s_OneByteArray = null;

            private static byte[] OneByteArray
            {
                get
                {
                    if (s_OneByteArray == null)
                    {
                        s_OneByteArray = new byte[1];
                    }

                    return s_OneByteArray;
                }
            }

            [ThreadStatic]
            private static byte[] s_TwoByteArray = null;

            private static byte[] TwoByteArray
            {
                get
                {
                    if (s_TwoByteArray == null)
                    {
                        s_TwoByteArray = new byte[2];
                    }

                    return s_TwoByteArray;
                }
            }

            [ThreadStatic]
            private static byte[] s_FourByteArray = null;

            private static byte[] FourByteArray
            {
                get
                {
                    if (s_FourByteArray == null)
                    {
                        s_FourByteArray = new byte[4];
                    }

                    return s_FourByteArray;
                }
            }

            [ThreadStatic]
            private static byte[] s_EightByteArray = null;

            private static byte[] EightByteArray
            {
                get
                {
                    if (s_EightByteArray == null)
                    {
                        s_EightByteArray = new byte[8];
                    }

                    return s_EightByteArray;
                }
            }

            public static unsafe byte[] GetBytes(bool value)
            {
                fixed (byte* p = OneByteArray)
                {
                    *(bool*) p = value;
                }

                return OneByteArray;
            }

            public static unsafe void GetBytes(bool value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(bool) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(bool*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(char value)
            {
                fixed (byte* p = TwoByteArray)
                {
                    *(char*) p = value;
                }

                return TwoByteArray;
            }

            public static unsafe void GetBytes(char value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(char) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(char*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(double value)
            {
                fixed (byte* p = EightByteArray)
                {
                    *(double*) p = value;
                }

                return EightByteArray;
            }

            public static unsafe void GetBytes(double value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(double) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(double*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(short value)
            {
                fixed (byte* p = TwoByteArray)
                {
                    *(short*) p = value;
                }

                return TwoByteArray;
            }

            public static unsafe void GetBytes(short value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(short) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(short*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(int value)
            {
                fixed (byte* p = FourByteArray)
                {
                    *(int*) p = value;
                }

                return FourByteArray;
            }

            public static unsafe void GetBytes(int value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(int) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(int*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(long value)
            {
                fixed (byte* p = EightByteArray)
                {
                    *(long*) p = value;
                }

                return EightByteArray;
            }

            public static unsafe void GetBytes(long value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(long) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(long*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(float value)
            {
                fixed (byte* p = FourByteArray)
                {
                    *(float*) p = value;
                }

                return FourByteArray;
            }

            public static unsafe void GetBytes(float value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(float) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(float*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(ushort value)
            {
                fixed (byte* p = TwoByteArray)
                {
                    *(ushort*) p = value;
                }

                return TwoByteArray;
            }

            public static unsafe void GetBytes(ushort value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(ushort) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(ushort*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(uint value)
            {
                fixed (byte* p = FourByteArray)
                {
                    *(uint*) p = value;
                }

                return FourByteArray;
            }

            public static unsafe void GetBytes(uint value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(uint) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(uint*) (p + offset) = value;
                }
            }

            public static unsafe byte[] GetBytes(ulong value)
            {
                fixed (byte* p = EightByteArray)
                {
                    *(ulong*) p = value;
                }

                return EightByteArray;
            }

            public static unsafe void GetBytes(ulong value, byte[] buffer, int offset)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || offset + sizeof(ulong) > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (byte* p = buffer)
                {
                    *(ulong*) (p + offset) = value;
                }
            }
        }
    }
}
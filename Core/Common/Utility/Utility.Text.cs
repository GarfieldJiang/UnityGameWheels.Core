using System;

namespace COL.UnityGameWheels.Core
{
    public static partial class Utility
    {
        /// <summary>
        /// Text utility.
        /// </summary>
        public static class Text
        {
            /// <summary>
            /// Format string.
            /// </summary>
            /// <param name="format">String format.</param>
            /// <param name="arg0">Arg 0.</param>
            /// <returns>Formatted string.</returns>
            public static string Format(string format, object arg0)
            {
                return FormatHelper(format, arg0);
            }

            /// <summary>
            /// Format string.
            /// </summary>
            /// <param name="format">String format.</param>
            /// <param name="arg0">Arg 0.</param>
            /// <param name="arg1">Arg 1.</param>
            /// <returns>Formatted string.</returns>
            public static string Format(string format, object arg0, object arg1)
            {
                return FormatHelper(format, arg0, arg1);
            }

            /// <summary>
            /// Format string.
            /// </summary>
            /// <param name="format">String format.</param>
            /// <param name="arg0">Arg 0.</param>
            /// <param name="arg1">Arg 1.</param>
            /// <param name="arg2">Arg 2.</param>
            /// <returns>Formatted string.</returns>
            public static string Format(string format, object arg0, object arg1, object arg2)
            {
                return FormatHelper(format, arg0, arg1, arg2);
            }

            /// <summary>
            /// Format string.
            /// </summary>
            /// <param name="format">String format.</param>
            /// <param name="args">Args.</param>
            /// <returns>Formatted string.</returns>
            public static string Format(string format, params object[] args)
            {
                if (args == null)
                {
                    // To preserve the original exception behavior, throw an exception about format if both
                    // args and format are null. The actual null check for format is in FormatHelper.
                    throw new ArgumentNullException((format == null) ? "format" : "args");
                }

                return FormatHelper(format, args);
            }

            private static string FormatHelper(string format, object arg0)
            {
                if (format == null)
                    throw new ArgumentNullException(nameof(format));

                return StringBuilderCache.GetStringAndRelease(
                    StringBuilderCache
                        .Acquire(format.Length + 8)
                        .AppendFormat(format, arg0));
            }

            private static string FormatHelper(string format, object arg0, object arg1)
            {
                if (format == null)
                    throw new ArgumentNullException(nameof(format));

                return StringBuilderCache.GetStringAndRelease(
                    StringBuilderCache
                        .Acquire(format.Length + 16)
                        .AppendFormat(format, arg0, arg1));
            }

            private static string FormatHelper(string format, object arg0, object arg1, object arg2)
            {
                if (format == null)
                    throw new ArgumentNullException(nameof(format));

                return StringBuilderCache.GetStringAndRelease(
                    StringBuilderCache
                        .Acquire(format.Length + 24)
                        .AppendFormat(format, arg0, arg1, arg2));
            }

            private static string FormatHelper(string format, params object[] args)
            {
                if (format == null)
                    throw new ArgumentNullException(nameof(format));

                return StringBuilderCache.GetStringAndRelease(
                    StringBuilderCache
                        .Acquire(format.Length + args.Length * 8)
                        .AppendFormat(format, args));
            }
        }
    }
}
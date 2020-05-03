using System;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Simple log interfaces.
    /// </summary>
    public static partial class CoreLog
    {
        private class DummyLoggerImpl : ILoggerImpl
        {
            private static DummyLoggerImpl s_Instance;

            public static DummyLoggerImpl Instance
            {
                get
                {
                    if (s_Instance == null)
                    {
                        s_Instance = new DummyLoggerImpl();
                    }

                    return s_Instance;
                }
            }

            public void WriteLog(LogLevel logLevel, object message)
            {
                // Empty.
            }

            public void WriteLog(LogLevel logLevel, object message, object context)
            {
                // Empty.
            }

            public void WriteException(Exception exception)
            {
                // Empty.
            }

            public void WriteException(Exception exception, object context)
            {
                // Empty.
            }
        }
    }
}
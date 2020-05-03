using System;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Simple logger interface.
    /// </summary>
    public interface ILoggerImpl
    {
        void WriteLog(LogLevel logLevel, object message);

        void WriteLog(LogLevel logLevel, object message, object context);

        void WriteException(Exception exception);

        void WriteException(Exception exception, object context);
    }
}
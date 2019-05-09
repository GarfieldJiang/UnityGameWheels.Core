namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Simple logger interface.
    /// </summary>
    public interface ILoggerImpl
    {
        void WriteLog(LogLevel logLevel, object message);

        void WriteLog(LogLevel logLevel, object message, object context);
    }
}
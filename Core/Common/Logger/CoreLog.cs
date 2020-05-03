using System;
using System.Diagnostics;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Simple log interfaces.
    /// </summary>
    public static partial class CoreLog
    {
        private static ILoggerImpl s_Logger = null;

        public static void SetLogger(ILoggerImpl logger)
        {
            s_Logger = logger;
        }

        private static ILoggerImpl Logger
        {
            get { return s_Logger ?? DummyLoggerImpl.Instance; }
        }

        [Conditional("LOG_DEBUG")]
        public static void Debug(object message)
        {
            Logger.WriteLog(LogLevel.Debug, message);
        }

        [Conditional("LOG_DEBUG")]
        public static void Debug(object message, object context)
        {
            Logger.WriteLog(LogLevel.Debug, message, context);
        }

        [Conditional("LOG_DEBUG")]
        public static void DebugFormat(string format, object arg)
        {
            Logger.WriteLog(LogLevel.Debug, Utility.Text.Format(format, arg));
        }

        [Conditional("LOG_DEBUG")]
        public static void DebugFormat(string format, object arg0, object arg1)
        {
            Logger.WriteLog(LogLevel.Debug, Utility.Text.Format(format, arg0, arg1));
        }

        [Conditional("LOG_DEBUG")]
        public static void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Logger.WriteLog(LogLevel.Debug, Utility.Text.Format(format, arg0, arg1, arg2));
        }

        [Conditional("LOG_DEBUG")]
        public static void DebugFormat(string format, params object[] args)
        {
            Logger.WriteLog(LogLevel.Debug, Utility.Text.Format(format, args));
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void Info(object message)
        {
            Logger.WriteLog(LogLevel.Info, message);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void Info(object message, object context)
        {
            Logger.WriteLog(LogLevel.Info, message, context);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void InfoFormat(string format, object arg)
        {
            Logger.WriteLog(LogLevel.Info, Utility.Text.Format(format, arg));
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void InfoFormat(string format, object arg0, object arg1)
        {
            Logger.WriteLog(LogLevel.Info, Utility.Text.Format(format, arg0, arg1));
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Logger.WriteLog(LogLevel.Info, Utility.Text.Format(format, arg0, arg1, arg2));
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void InfoFormat(string format, params object[] args)
        {
            Logger.WriteLog(LogLevel.Info, Utility.Text.Format(format, args));
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void Warning(object message)
        {
            Logger.WriteLog(LogLevel.Warning, message);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void Warning(object message, object context)
        {
            Logger.WriteLog(LogLevel.Warning, message, context);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void WarningFormat(string format, object arg)
        {
            Logger.WriteLog(LogLevel.Warning, Utility.Text.Format(format, arg));
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void WarningFormat(string format, object arg0, object arg1)
        {
            Logger.WriteLog(LogLevel.Warning, Utility.Text.Format(format, arg0, arg1));
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void WarningFormat(string format, object arg0, object arg1, object arg2)
        {
            Logger.WriteLog(LogLevel.Warning, Utility.Text.Format(format, arg0, arg1, arg2));
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void WarningFormat(string format, params object[] args)
        {
            Logger.WriteLog(LogLevel.Warning, Utility.Text.Format(format, args));
        }

        public static void Error(object message)
        {
            Logger.WriteLog(LogLevel.Error, message);
        }

        public static void Error(object message, object context)
        {
            Logger.WriteLog(LogLevel.Error, message, context);
        }

        public static void ErrorFormat(string format, object arg)
        {
            Logger.WriteLog(LogLevel.Error, Utility.Text.Format(format, arg));
        }

        public static void ErrorFormat(string format, object arg0, object arg1)
        {
            Logger.WriteLog(LogLevel.Error, Utility.Text.Format(format, arg0, arg1));
        }

        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Logger.WriteLog(LogLevel.Error, Utility.Text.Format(format, arg0, arg1, arg2));
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            Logger.WriteLog(LogLevel.Error, Utility.Text.Format(format, args));
        }

        public static void Fatal(object message)
        {
            Logger.WriteLog(LogLevel.Fatal, message);
        }

        public static void Fatal(object message, object context)
        {
            Logger.WriteLog(LogLevel.Fatal, message, context);
        }

        public static void FatalFormat(string format, object arg)
        {
            Logger.WriteLog(LogLevel.Fatal, Utility.Text.Format(format, arg));
        }

        public static void FatalFormat(string format, object arg0, object arg1)
        {
            Logger.WriteLog(LogLevel.Fatal, Utility.Text.Format(format, arg0, arg1));
        }

        public static void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Logger.WriteLog(LogLevel.Fatal, Utility.Text.Format(format, arg0, arg1, arg2));
        }

        public static void FatalFormat(string format, params object[] args)
        {
            Logger.WriteLog(LogLevel.Fatal, Utility.Text.Format(format, args));
        }

        public static void Exception(Exception e)
        {
            Logger.WriteException(e);
        }

        public static void Exception(Exception e, object context)
        {
            Logger.WriteException(e, context);
        }
    }
}
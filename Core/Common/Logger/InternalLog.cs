using System;
using System.Diagnostics;

namespace COL.UnityGameWheels.Core
{
    internal static class InternalLog
    {
        [Conditional("LOG_DEBUG")]
        public static void Debug(object message)
        {
            CoreLog.Debug(message);
        }

        [Conditional("LOG_DEBUG")]
        public static void Debug(object message, Object context)
        {
            CoreLog.Debug(message, context);
        }

        [Conditional("LOG_DEBUG")]
        public static void DebugFormat(string format, object arg)
        {
            CoreLog.DebugFormat(format, arg);
        }

        [Conditional("LOG_DEBUG")]
        public static void DebugFormat(string format, object arg0, object arg1)
        {
            CoreLog.DebugFormat(format, arg0, arg1);
        }

        [Conditional("LOG_DEBUG")]
        public static void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            CoreLog.DebugFormat(format, arg0, arg1, arg2);
        }

        [Conditional("LOG_DEBUG")]
        public static void DebugFormat(string format, params object[] args)
        {
            CoreLog.DebugFormat(format, args);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void Info(object message)
        {
            CoreLog.Info(message);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void Info(object message, Object context)
        {
            CoreLog.Info(message, context);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void InfoFormat(string format, object arg)
        {
            CoreLog.InfoFormat(format, arg);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void InfoFormat(string format, object arg0, object arg1)
        {
            CoreLog.InfoFormat(format, arg0, arg1);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            CoreLog.InfoFormat(format, arg0, arg1, arg2);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO")]
        public static void InfoFormat(string format, params object[] args)
        {
            CoreLog.InfoFormat(format, args);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void Warning(object message)
        {
            CoreLog.Warning(message);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void Warning(object message, Object context)
        {
            CoreLog.Warning(message, context);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void WarningFormat(string format, object arg)
        {
            CoreLog.WarningFormat(format, arg);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void WarningFormat(string format, object arg0, object arg1)
        {
            CoreLog.WarningFormat(format, arg0, arg1);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void WarningFormat(string format, object arg0, object arg1, object arg2)
        {
            CoreLog.WarningFormat(format, arg0, arg1, arg2);
        }

        [Conditional("LOG_DEBUG"), Conditional("LOG_INFO"), Conditional("LOG_WARNING")]
        public static void WarningFormat(string format, params object[] args)
        {
            CoreLog.WarningFormat(format, args);
        }

        public static void Error(object message)
        {
            CoreLog.Error(message);
        }

        public static void Error(object message, Object context)
        {
            CoreLog.Error(message, context);
        }

        public static void ErrorFormat(string format, object arg)
        {
            CoreLog.ErrorFormat(format, arg);
        }

        public static void ErrorFormat(string format, object arg0, object arg1)
        {
            CoreLog.ErrorFormat(format, arg0, arg1);
        }

        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            CoreLog.ErrorFormat(format, arg0, arg1, arg2);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            CoreLog.ErrorFormat(format, args);
        }

        public static void Fatal(object message)
        {
            CoreLog.Fatal(message);
        }

        public static void Fatal(object message, Object context)
        {
            CoreLog.Fatal(message, context);
        }

        public static void FatalFormat(string format, object arg)
        {
            CoreLog.FatalFormat(format, arg);
        }

        public static void FatalFormat(string format, object arg0, object arg1)
        {
            CoreLog.FatalFormat(format, arg0, arg1);
        }

        public static void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            CoreLog.FatalFormat(format, arg0, arg1, arg2);
        }

        public static void FatalFormat(string format, params object[] args)
        {
            CoreLog.FatalFormat(format, args);
        }

        public static void Exception(Exception e)
        {
            CoreLog.Exception(e);
        }

        public static void Exception(Exception e, Object context)
        {
            CoreLog.Exception(e, context);
        }
    }
}
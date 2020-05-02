using System;
using System.Reflection;

namespace COL.UnityGameWheels.Core
{
    public static partial class Guard
    {
        public static void RequireFalse<TException>(bool booleanExpression, string message = null, Exception innerException = null)
            where TException : Exception, new()
        {
            if (!booleanExpression)
            {
                return;
            }

            throw CreateException(typeof(TException), message, innerException);
        }

        public static void RequireTrue<TException>(bool booleanExpression, string message = null, Exception innerException = null)
            where TException : Exception, new()
        {
            if (booleanExpression)
            {
                return;
            }

            throw CreateException(typeof(TException), message, innerException);
        }

        public static void RequireNotNullOrEmpty<TException>(string stringVal, string message = null, Exception innerException = null)
            where TException : Exception, new()
        {
            RequireTrue<TException>(!string.IsNullOrEmpty(stringVal), message, innerException);
        }

        public static void RequireNotNull<TException>(object obj, string message = null, Exception innerException = null)
            where TException : Exception, new()
        {
            RequireTrue<TException>(obj != null, message, innerException);
        }

        public static void RequireNull<TException>(object obj, string message = null, Exception innerException = null)
            where TException : Exception, new()
        {
            RequireTrue<TException>(obj == null, message, innerException);
        }

        private static Exception CreateException(Type exceptionType, string message, Exception innerException)
        {
            var ret = (Exception)Activator.CreateInstance(exceptionType);
            if (message != null)
            {
                SetExceptionField(ret, "_message", message);
            }

            if (innerException != null)
            {
                SetExceptionField(ret, "_innerException", innerException);
            }

            return ret;
        }

        private static void SetExceptionField(Exception exception, string fieldName, object val)
        {
            var fieldInfo = typeof(Exception).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null && fieldInfo.FieldType.IsInstanceOfType(val))
            {
                fieldInfo.SetValue(exception, val);
            }
        }
    }
}
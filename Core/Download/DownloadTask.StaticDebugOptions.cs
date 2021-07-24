using System;
using System.IO;

namespace COL.UnityGameWheels.Core
{
    public sealed partial class DownloadTask
    {
        public static class StaticDebugOptions
        {
            [Flags]
            public enum IOExceptionScenario
            {
                None = 0,
                OnOpenFile = 1,
                OnWriteFile = 2,
                OnMoveFile = 4,
            }

            public enum IOExceptionMsg
            {
                Normal = 0,
                DiskFull = 1,
            }

            private static int s_MaxThrowTimes = 5;
            private static int s_ThrowTimes = 0;

            private static IOExceptionScenario s_IOExceptionScenario;

            private static IOExceptionMsg s_IOExceptionMsg;

            public static void SetIOExceptionScenario(IOExceptionScenario scenario, int maxThrowTimes)
            {
                s_IOExceptionScenario = scenario;
                s_ThrowTimes = 0;
                s_MaxThrowTimes = maxThrowTimes > 0 ? maxThrowTimes : 1;
            }

            public static void SetIOExceptionMsg(IOExceptionMsg msg)
            {
                s_IOExceptionMsg = msg;
            }

            internal static void ExceptionIfNeeded(IOExceptionScenario scenario)
            {
                if ((s_IOExceptionScenario & scenario) != scenario)
                {
                    return;
                }

                ++s_ThrowTimes;
                if (s_ThrowTimes >= s_MaxThrowTimes)
                {
                    s_IOExceptionScenario = IOExceptionScenario.None;
                }

                throw new IOException(s_IOExceptionMsg == IOExceptionMsg.DiskFull ? "Disk full." : "Test IO exception msg.");
            }
        }
    }
}
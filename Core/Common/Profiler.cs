#if PROFILING
using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    public static class Profiler
    {
        private static Stack<DateTime> s_Stack;

        private static Stack<DateTime> Stack
        {
            get
            {
                if (s_Stack == null)
                {
                    s_Stack = new Stack<DateTime>();
                }

                return s_Stack;
            }
        }

        public static void BeginSample()
        {
            Stack.Push(DateTime.UtcNow);
        }

        public static TimeSpan EndSample()
        {
            var peek = Stack.Pop();
            var ret = DateTime.UtcNow - peek;
            return ret;
        }
    }
}
#endif
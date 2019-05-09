namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// Statistics of a reference pool.
    /// </summary>
    public struct RefPoolStatistics
    {
        /// <summary>
        /// How many objects have been created.
        /// </summary>
        public int CreateCount { get; internal set; }

        /// <summary>
        /// How many objects have been acquired.
        /// </summary>
        public int AcquireCount { get; internal set; }

        /// <summary>
        /// How many objects have been released.
        /// </summary>
        public int ReleaseCount { get; internal set; }

        /// <summary>
        /// How many objects have been dropped.
        /// </summary>
        public int DropCount { get; internal set; }

        internal static RefPoolStatistics FromInternal(RefPoolStatisticsInternal original)
        {
            return new RefPoolStatistics
            {
                CreateCount = original.CreateCount,
                AcquireCount = original.AcquireCount,
                ReleaseCount = original.ReleaseCount,
                DropCount = original.DropCount,
            };
        }
    }
}
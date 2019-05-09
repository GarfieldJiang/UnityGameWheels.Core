namespace COL.UnityGameWheels.Core.Asset
{
    public abstract class BaseCacheQuery
    {
        public string Path { get; internal set; }

        public string ErrorMessage { get; internal set; }

        public float LoadingProgress { get; internal set; }

        public int RetainCount { get; internal set; }
    }
}

namespace COL.UnityGameWheels.Core
{
    public struct TimeStruct
    {
        public readonly float DeltaTime;
        public readonly float UnscaledDeltaTime;
        public readonly float Time;
        public readonly float UnscaledTime;

        public TimeStruct(float deltaTime, float unscaledDeltaTime, float time, float unscaledTime)
        {
            DeltaTime = deltaTime;
            UnscaledDeltaTime = unscaledDeltaTime;
            Time = time;
            UnscaledTime = unscaledTime;
        }
    }
}
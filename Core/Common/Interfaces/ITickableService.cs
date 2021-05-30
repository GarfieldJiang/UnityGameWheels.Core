namespace COL.UnityGameWheels.Core
{
    public interface ITickableService
    {
        bool IsTicking { get; }

        bool StartTicking();

        bool StopTicking();
    }
}
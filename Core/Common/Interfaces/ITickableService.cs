namespace COL.UnityGameWheels.Core
{
    public interface ITickableService
    {
        bool IsTicking { get; }

        int TickOrder { get; }

        bool StartTicking();

        bool StopTicking();

        void RefreshTickOrder(int newOrder);
    }
}
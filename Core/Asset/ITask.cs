namespace COL.UnityGameWheels.Core.Asset
{
    /// <summary>
    /// Simple task interface.
    /// </summary>
    public interface ITask
    {
        bool IsDone { get; }

        float Progress { get; }

        string ErrorMessage { get; }

        void OnReset();

        void OnStart();

        void OnUpdate(TimeStruct timeStruct);
    }
}
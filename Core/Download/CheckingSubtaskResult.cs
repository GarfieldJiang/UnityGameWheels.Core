namespace COL.UnityGameWheels.Core
{
    public sealed partial class DownloadTask
    {
        private struct CheckingSubtaskResult
        {
            public DownloadErrorCode? ErrorCode;
            public string ErrorMessage;
        }
    }
}
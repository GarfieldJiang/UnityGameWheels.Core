namespace COL.UnityGameWheels.Core
{
    public partial class EventService
    {
        private readonly struct SenderEventPair
        {
            public readonly object Sender;
            public readonly BaseEventArgs EventArgs;

            public SenderEventPair(object sender, BaseEventArgs e)
            {
                Sender = sender;
                EventArgs = e;
            }
        }
    }
}
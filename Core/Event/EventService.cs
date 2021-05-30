using System.Collections.Concurrent;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// The default implementation of an event manager.
    /// </summary> 
    public partial class EventService : TickableService, IEventService
    {
        [RequireThreadSafeRefPool]
        private class CopiedListenerCollection
        {
            internal readonly List<OnHearEvent> InnerCollection = new List<OnHearEvent>(4);

            internal void Reset()
            {
                InnerCollection.Clear();
            }
        }

        private int? m_MainThreadId = null;
        private readonly Dictionary<int, LinkedList<OnHearEvent>> m_Listeners = new Dictionary<int, LinkedList<OnHearEvent>>();
        private readonly ConcurrentQueue<SenderEventPair> m_EventQueue = new ConcurrentQueue<SenderEventPair>();
        private readonly Queue<SenderEventPair> m_UpdateEventQueue = new Queue<SenderEventPair>();
        private readonly IEventArgsReleaser m_EventArgsReleaser = null;
        private readonly IRefPoolService m_RefPoolService = null;
        private readonly IRefPool<CopiedListenerCollection> m_CopiedListenerCollectionPool = null;

        public EventService(ITickService tickService, IRefPoolService refPoolService, IEventArgsReleaser eventArgsReleaser)
            : base(tickService)
        {
            m_RefPoolService = refPoolService;
            m_CopiedListenerCollectionPool = m_RefPoolService.Add<CopiedListenerCollection>(4);
            m_EventArgsReleaser = eventArgsReleaser;
        }

        /// <inheritdoc />
        public int MainThreadId
        {
            set
            {
                if (m_MainThreadId != null)
                {
                    throw new System.InvalidOperationException("Main thread ID has already been set.");
                }

                m_MainThreadId = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                lock (m_Listeners)
                {
                    m_Listeners.Clear();
                }

                while (m_EventQueue.TryDequeue(out _))
                {
                }

                m_UpdateEventQueue.Clear();
            }
        }

        /// <summary>
        /// Add a event listener callback.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="onHearEvent">Callback when hearing the event.</param>
        public void AddEventListener(int eventId, OnHearEvent onHearEvent)
        {
            CheckMainThreadOrThrow();
            CheckListenerOrThrow(onHearEvent);
            lock (m_Listeners)
            {
                EnsureListenerCollection(eventId).AddLast(onHearEvent);
            }
        }

        /// <summary>
        /// Remove a event listener callback.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="onHearEvent">Callback when hearing the event.</param>
        public void RemoveEventListener(int eventId, OnHearEvent onHearEvent)
        {
            CheckMainThreadOrThrow();
            CheckListenerOrThrow(onHearEvent);
            lock (m_Listeners)
            {
                EnsureListenerCollection(eventId).Remove(onHearEvent);
            }
        }

        /// <summary>
        /// Send an event to listeners.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        public void SendEvent(object sender, BaseEventArgs e)
        {
            m_EventQueue.Enqueue(new SenderEventPair(sender, e));
        }

        /// <summary>
        /// Send an event to listeners without any delay.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event itself.</param>
        public void SendEventNow(object sender, BaseEventArgs eventArgs)
        {
            CheckMainThreadOrThrow();
            var copiedListenerCollection = PrepareCopiedListenerCollection(eventArgs);

            try
            {
                foreach (var listener in copiedListenerCollection.InnerCollection)
                {
                    listener(sender, eventArgs);
                }
            }
            finally
            {
                copiedListenerCollection.Reset();
                m_CopiedListenerCollectionPool.Release(copiedListenerCollection);
            }
        }

        public override bool StartTicking()
        {
            CheckMainThreadOrThrow();
            return base.StartTicking();
        }

        public override bool StopTicking()
        {
            CheckMainThreadOrThrow();
            return base.StopTicking();
        }

        private CopiedListenerCollection PrepareCopiedListenerCollection(BaseEventArgs eventArgs)
        {
            CopiedListenerCollection copiedListenerCollection = m_RefPoolService.GetOrAdd<CopiedListenerCollection>().Acquire();
            lock (m_Listeners)
            {
                copiedListenerCollection.InnerCollection.AddRange(EnsureListenerCollection(eventArgs.EventId));
            }

            return copiedListenerCollection;
        }

        protected override void OnUpdate(TimeStruct timeStruct)
        {
            CheckMainThreadOrThrow();
            m_UpdateEventQueue.Clear();
            while (m_EventQueue.TryDequeue(out var item))
            {
                m_UpdateEventQueue.Enqueue(item);
            }

            while (m_UpdateEventQueue.Count > 0)
            {
                var senderEventPair = m_UpdateEventQueue.Dequeue();
                SendEventNow(senderEventPair.Sender, senderEventPair.EventArgs);
            }
        }

        private void CheckListenerOrThrow(OnHearEvent onHearEvent)
        {
            if (onHearEvent == null)
            {
                throw new System.ArgumentNullException(nameof(onHearEvent));
            }
        }

        private LinkedList<OnHearEvent> EnsureListenerCollection(int eventId)
        {
            if (!m_Listeners.TryGetValue(eventId, out var listeners))
            {
                listeners = new LinkedList<OnHearEvent>();
                m_Listeners.Add(eventId, listeners);
            }

            return listeners;
        }

        private void CheckMainThreadOrThrow()
        {
            if (m_MainThreadId == null)
            {
                throw new System.InvalidOperationException("Main thread ID not set yet.");
            }

            var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (m_MainThreadId != currentThreadId)
            {
                throw new System.InvalidOperationException(Utility.Text.Format("Current thread {0} is not the main thread {1}.",
                    currentThreadId, m_MainThreadId.Value));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// The default implementation of an event manager.
    /// </summary>
    public partial class EventService : BaseLifeCycleService, IEventService
    {
        private int? m_MainThreadId = null;
        private Dictionary<int, LinkedList<OnHearEvent>> m_Listeners = null;
        private List<OnHearEvent> m_CopiedListenerCollection = null;
        private bool m_CopiedListenerCollectionIsBeingUsed = false;
        private readonly Queue<SenderEventPair> m_EventQueue = new Queue<SenderEventPair>();
        private Queue<SenderEventPair> m_UpdateEventQueue = null;
        private IEventArgsReleaser m_EventArgsReleaser = new DefaultEventArgsReleaser();

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

        /// <inheritdoc />
        [Ioc.Inject]
        public IEventArgsReleaser EventArgsReleaser
        {
            get => m_EventArgsReleaser;
            set
            {
                CheckMainThreadOrThrow();
                m_EventArgsReleaser = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <inheritdoc />
        public override void OnInit()
        {
            CheckMainThreadOrThrow();
            base.OnInit();
            m_Listeners = new Dictionary<int, LinkedList<OnHearEvent>>();
            m_CopiedListenerCollection = new List<OnHearEvent>();
            m_UpdateEventQueue = new Queue<SenderEventPair>();
        }

        /// <inheritdoc />
        public override void OnShutdown()
        {
            CheckMainThreadOrThrow();
            CheckStateOrThrow();
            foreach (var kv in m_Listeners)
            {
                kv.Value.Clear();
            }

            m_Listeners.Clear();
            m_CopiedListenerCollection.Clear();

            lock (m_EventQueue)
            {
                m_EventQueue.Clear();
            }

            base.OnShutdown();
        }

        /// <summary>
        /// Add a event listener callback.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="onHearEvent">Callback when hearing the event.</param>
        public void AddEventListener(int eventId, OnHearEvent onHearEvent)
        {
            CheckMainThreadOrThrow();
            CheckStateOrThrow();
            CheckListenerOrThrow(onHearEvent);
            EnsureListenerCollection(eventId).AddLast(onHearEvent);
        }

        /// <summary>
        /// Remove a event listener callback.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="onHearEvent">Callback when hearing the event.</param>
        public void RemoveEventListener(int eventId, OnHearEvent onHearEvent)
        {
            CheckMainThreadOrThrow();
            CheckStateOrThrow();
            CheckListenerOrThrow(onHearEvent);
            EnsureListenerCollection(eventId).Remove(onHearEvent);
        }

        /// <summary>
        /// Send an event to listeners.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        public void SendEvent(object sender, BaseEventArgs e)
        {
            CheckStateOrThrow();
            lock (m_EventQueue)
            {
                m_EventQueue.Enqueue(new SenderEventPair(sender, e));
            }
        }

        /// <summary>
        /// Send an event to listeners without any delay.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event itself.</param>
        public void SendEventNow(object sender, BaseEventArgs eventArgs)
        {
            CheckMainThreadOrThrow();
            CheckStateOrThrow();
            var copiedListenerCollection = PrepareCopiedListenerCollection(eventArgs);

            try
            {
                foreach (var listener in copiedListenerCollection)
                {
                    listener(sender, eventArgs);
                }
            }
            finally
            {
                ClearCopiedListenerCollection(copiedListenerCollection);
                m_EventArgsReleaser.Release(eventArgs);
            }
        }

        private void ClearCopiedListenerCollection(List<OnHearEvent> copiedListenerCollection)
        {
            if (copiedListenerCollection == m_CopiedListenerCollection)
            {
                m_CopiedListenerCollectionIsBeingUsed = false;
            }

            copiedListenerCollection.Clear();
        }

        private List<OnHearEvent> PrepareCopiedListenerCollection(BaseEventArgs eventArgs)
        {
            List<OnHearEvent> copiedListenerCollection;
            if (m_CopiedListenerCollectionIsBeingUsed)
            {
                copiedListenerCollection = new List<OnHearEvent>();
            }
            else
            {
                m_CopiedListenerCollectionIsBeingUsed = true;
                copiedListenerCollection = m_CopiedListenerCollection;
            }

            copiedListenerCollection.AddRange(EnsureListenerCollection(eventArgs.EventId));
            return copiedListenerCollection;
        }

        /// <inheritdoc />
        public void OnUpdate(TimeStruct timeStruct)
        {
            CheckMainThreadOrThrow();
            m_UpdateEventQueue.Clear();
            lock (m_EventQueue)
            {
                while (m_EventQueue.Count > 0)
                {
                    m_UpdateEventQueue.Enqueue(m_EventQueue.Dequeue());
                }
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
            LinkedList<OnHearEvent> listeners;
            if (!m_Listeners.TryGetValue(eventId, out listeners))
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
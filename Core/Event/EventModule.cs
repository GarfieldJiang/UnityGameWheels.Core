using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    /// <summary>
    /// The default implementation of an event manager.
    /// </summary>
    public partial class EventModule : BaseModule, IEventModule
    {
        private int? m_MainThreadId = null;
        private Dictionary<int, LinkedList<OnHearEvent>> m_Listeners = null;
        private List<OnHearEvent> m_CopiedListenerCollection = null;
        private Queue<SenderEventPair> m_EventQueue = null;
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
        public override void Init()
        {
            CheckMainThreadOrThrow();
            base.Init();
            m_Listeners = new Dictionary<int, LinkedList<OnHearEvent>>();
            m_CopiedListenerCollection = new List<OnHearEvent>();
            m_EventQueue = new Queue<SenderEventPair>();
            m_UpdateEventQueue = new Queue<SenderEventPair>();
        }

        /// <inheritdoc />
        public override void ShutDown()
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

            base.ShutDown();
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
            m_CopiedListenerCollection.Clear();
            m_CopiedListenerCollection.AddRange(EnsureListenerCollection(eventArgs.EventId));
            try
            {
                foreach (var listener in m_CopiedListenerCollection)
                {
                    listener(sender, eventArgs);
                }
            }
            finally
            {
                m_CopiedListenerCollection.Clear();
                m_EventArgsReleaser.Release(eventArgs);
            }
        }

        /// <summary>
        /// Generic tick method.
        /// </summary>
        /// <param name="timeStruct">Time struct.</param>
        public override void Update(TimeStruct timeStruct)
        {
            CheckMainThreadOrThrow();
            base.Update(timeStruct);
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
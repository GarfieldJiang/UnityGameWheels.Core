using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class EventTests
    {
        private EventService m_EventService = null;
        private ITickService m_TickService = null;
        private IRefPoolService m_RefPoolService = null;

        private class OneSimpleEventArgs : BaseEventArgs
        {
            public static readonly int TheEventId = EventIdToTypeMap.Generate<OneSimpleEventArgs>();

            public override int EventId => TheEventId;
        }

        private class AnotherSimpleEventArgs : BaseEventArgs
        {
            public static readonly int TheEventId = EventIdToTypeMap.Generate<AnotherSimpleEventArgs>();

            public override int EventId => TheEventId;
        }

        private class NoIdEventArgs : BaseEventArgs
        {
            public override int EventId => throw new NotImplementedException();
        }

        private abstract class AbstractEventArgs : BaseEventArgs
        {
            // Empty.
        }

        [Test]
        public void TestEventIdsAndTypes()
        {
            Assert.AreNotEqual(OneSimpleEventArgs.TheEventId, AnotherSimpleEventArgs.TheEventId);
            Assert.IsTrue(EventIdToTypeMap.HasEventId(OneSimpleEventArgs.TheEventId));
            Assert.IsTrue(EventIdToTypeMap.HasEventId(AnotherSimpleEventArgs.TheEventId));
            Assert.IsTrue(EventIdToTypeMap.HasEventType<OneSimpleEventArgs>());
            Assert.IsTrue(EventIdToTypeMap.HasEventType<AnotherSimpleEventArgs>());
            Assert.IsFalse(EventIdToTypeMap.HasEventId(10000000));
            Assert.IsFalse(EventIdToTypeMap.HasEventType<NoIdEventArgs>());
        }

        [Test]
        public void TestGenerateEventIdError()
        {
            Assert.Throws<ArgumentException>(() => EventIdToTypeMap.Generate<AbstractEventArgs>());
        }

        [Test]
        public void TestSendEvent()
        {
            int firstEventCount = 0;
            int secondEventCount = 0;

            void OnHearFirstEvent(object sender, BaseEventArgs e)
            {
                Assert.AreSame(sender, this);
                Assert.True(e is OneSimpleEventArgs);
                firstEventCount++;
            }

            void OnHearSecondEvent(object sender, BaseEventArgs e)
            {
                Assert.AreSame(sender, this);
                Assert.True(e is AnotherSimpleEventArgs);
                secondEventCount++;
            }

            m_EventService.AddEventListener(OneSimpleEventArgs.TheEventId, OnHearFirstEvent);

            m_EventService.SendEvent(this, new OneSimpleEventArgs());
            m_EventService.SendEvent(this, new AnotherSimpleEventArgs());

            Assert.AreEqual(0, firstEventCount);
            Assert.AreEqual(0, secondEventCount);

            ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(0f, 0f, 0f, 0f));

            Assert.AreEqual(1, firstEventCount);
            Assert.AreEqual(0, secondEventCount);

            m_EventService.AddEventListener(AnotherSimpleEventArgs.TheEventId, OnHearSecondEvent);

            m_EventService.AddEventListener(OneSimpleEventArgs.TheEventId, OnHearFirstEvent);

            m_EventService.SendEvent(this, new OneSimpleEventArgs());
            m_EventService.SendEvent(this, new AnotherSimpleEventArgs());

            Assert.AreEqual(1, firstEventCount);
            Assert.AreEqual(0, secondEventCount);

            ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(0f, 0f, 0f, 0f));

            Assert.AreEqual(3, firstEventCount);
            Assert.AreEqual(1, secondEventCount);

            m_EventService.RemoveEventListener(OneSimpleEventArgs.TheEventId, OnHearFirstEvent);
            m_EventService.RemoveEventListener(AnotherSimpleEventArgs.TheEventId, OnHearSecondEvent);

            m_EventService.SendEvent(this, new OneSimpleEventArgs());
            m_EventService.SendEvent(this, new AnotherSimpleEventArgs());

            ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(0f, 0f, 0f, 0f));

            Assert.AreEqual(4, firstEventCount);
            Assert.AreEqual(1, secondEventCount);
        }

        [Test]
        public void TestSendEventNow()
        {
            int firstEventCount = 0;

            void OnHearFirstEvent(object sender, BaseEventArgs e)
            {
                Assert.AreSame(sender, this);
                Assert.True(e is OneSimpleEventArgs);
                firstEventCount++;
            }

            m_EventService.AddEventListener(OneSimpleEventArgs.TheEventId, OnHearFirstEvent);
            m_EventService.SendEventNow(this, new OneSimpleEventArgs());
            Assert.AreEqual(1, firstEventCount);
            ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(0f, 0f, 0f, 0f));
            Assert.AreEqual(1, firstEventCount);
        }

        [Test]
        public void TestRemoveUnaddedListener()
        {
            OnHearEvent onHearFirstEvent = (sender, e) => { };
            m_EventService.RemoveEventListener(OneSimpleEventArgs.TheEventId, onHearFirstEvent);
        }

        [Test]
        public void TestAddRemoveNullListener()
        {
            Assert.Throws<ArgumentNullException>(() => { m_EventService.AddEventListener(OneSimpleEventArgs.TheEventId, null); });

            Assert.Throws<ArgumentNullException>(() => { m_EventService.RemoveEventListener(OneSimpleEventArgs.TheEventId, null); });
        }

        [Test]
        public void TestStartTickingBeforeSettingMainThreadId()
        {
            using (EventService anotherEventService = new EventService(new MockTickService(), new MockRefPoolService(), new DefaultEventArgsReleaser()))
            {
                Assert.Throws<InvalidOperationException>(() => { anotherEventService.StartTicking(); });
            }
        }


        [Test]
        public void TestUseInAnotherThread()
        {
            {
                int exceptionsCaught = 0;
                var thread = new Thread(() =>
                {
                    try
                    {
                        m_EventService.AddEventListener(OneSimpleEventArgs.TheEventId, (sender, e) => { });
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionsCaught++;
                    }

                    try
                    {
                        m_EventService.RemoveEventListener(OneSimpleEventArgs.TheEventId, (sender, e) => { });
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionsCaught++;
                    }

                    try
                    {
                        m_EventService.SendEventNow(null, new OneSimpleEventArgs());
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionsCaught++;
                    }
                });

                thread.Start();
                thread.Join();
                Assert.AreEqual(3, exceptionsCaught);
            }

            {
                int exceptionsCaught = 0;
                int eventsReceived = 0;
                int threadCount = 100;
                int eventCount = 100;
                object lockObj = new object();
                m_EventService.AddEventListener(OneSimpleEventArgs.TheEventId, (sender, o) => { eventsReceived++; });
                var threads = new List<Thread>();
                for (var i = 0; i < threadCount; i++)
                {
                    var thread = new Thread(() =>
                    {
                        for (var j = 0; j < eventCount; j++)
                        {
                            try
                            {
                                m_EventService.SendEvent(null, new OneSimpleEventArgs());
                            }
                            catch (InvalidOperationException)
                            {
                                lock (lockObj)
                                {
                                    exceptionsCaught++;
                                }
                            }
                        }
                    });
                    threads.Add(thread);
                }

                foreach (var thread in threads)
                {
                    thread.Start();
                    thread.Join();
                }

                Assert.AreEqual(0, eventsReceived);
                ((MockTickService)m_TickService).ManualUpdate(new TimeStruct(0f, 0f, 0f, 0f));
                Assert.AreEqual(threadCount * eventCount, eventsReceived);
                Assert.AreEqual(0, exceptionsCaught);
            }
        }

        [Test]
        public void TestSendInSendNow()
        {
            var eventListeners = new List<OnHearEvent>();
            var eventRecorder = new List<int>();
            bool hasRemovedEventListener2 = false;
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                eventListeners.Add((_, e) =>
                {
                    eventRecorder.Add(index);
                    if (index != 1)
                    {
                        return;
                    }

                    if (hasRemovedEventListener2)
                    {
                        return;
                    }

                    hasRemovedEventListener2 = true;
                    // Remove eventListeners[2] and broadcast the event again.
                    m_EventService.RemoveEventListener(OneSimpleEventArgs.TheEventId, eventListeners[index + 1]);
                    m_EventService.SendEvent(null, new OneSimpleEventArgs());
                });
            }

            foreach (var eventListener in eventListeners)
            {
                m_EventService.AddEventListener(OneSimpleEventArgs.TheEventId, eventListener);
            }

            m_EventService.SendEventNow(null, new OneSimpleEventArgs());
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, eventRecorder);
            ((MockTickService)m_TickService).ManualUpdate(new TimeStruct());
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 0, 1, 3 }, eventRecorder);
        }

        [Test]
        public void TestSendNowInSendNow()
        {
            var eventListeners = new List<OnHearEvent>();
            var eventRecorder = new List<int>();
            bool hasRemovedEventListener2 = false;
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                eventListeners.Add((_, e) =>
                {
                    eventRecorder.Add(index);
                    if (index != 1)
                    {
                        return;
                    }

                    if (hasRemovedEventListener2)
                    {
                        return;
                    }

                    hasRemovedEventListener2 = true;
                    // Remove eventListeners[2] and broadcast the event again.
                    m_EventService.RemoveEventListener(OneSimpleEventArgs.TheEventId, eventListeners[index + 1]);
                    m_EventService.SendEventNow(null, new OneSimpleEventArgs());
                });
            }

            foreach (var eventListener in eventListeners)
            {
                m_EventService.AddEventListener(OneSimpleEventArgs.TheEventId, eventListener);
            }

            m_EventService.SendEventNow(null, new OneSimpleEventArgs());
            CollectionAssert.AreEqual(new[] { 0, 1, 0, 1, 3, 2, 3 }, eventRecorder);
        }

        [SetUp]
        public void SetUp()
        {
            Assert.IsNull(m_EventService);
            m_TickService = new MockTickService();
            m_RefPoolService = new MockRefPoolService();
            m_EventService = new EventService(m_TickService, m_RefPoolService, new DefaultEventArgsReleaser())
            {
                MainThreadId = Thread.CurrentThread.ManagedThreadId
            };
            m_EventService.StartTicking();
        }

        [TearDown]
        public void TearDown()
        {
            Assert.IsNotNull(m_EventService);
            m_EventService.Dispose();
            m_EventService = null;
            m_TickService = null;
            m_RefPoolService = null;
        }
    }
}
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class EventTests
    {
        private IEventModule m_EventModule = null;

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

            OnHearEvent onHearFirstEvent = (sender, e) =>
            {
                Assert.AreSame(sender, this);
                Assert.True(e is OneSimpleEventArgs);
                firstEventCount++;
            };

            OnHearEvent onHearSecondEvent = (sender, e) =>
            {
                Assert.AreSame(sender, this);
                Assert.True(e is AnotherSimpleEventArgs);
                secondEventCount++;
            };

            m_EventModule.AddEventListener(OneSimpleEventArgs.TheEventId, onHearFirstEvent);

            m_EventModule.SendEvent(this, new OneSimpleEventArgs());
            m_EventModule.SendEvent(this, new AnotherSimpleEventArgs());

            Assert.AreEqual(0, firstEventCount);
            Assert.AreEqual(0, secondEventCount);

            m_EventModule.Update(new TimeStruct(0f, 0f, 0f, 0f));

            Assert.AreEqual(1, firstEventCount);
            Assert.AreEqual(0, secondEventCount);

            m_EventModule.AddEventListener(AnotherSimpleEventArgs.TheEventId, onHearSecondEvent);

            m_EventModule.AddEventListener(OneSimpleEventArgs.TheEventId, onHearFirstEvent);

            m_EventModule.SendEvent(this, new OneSimpleEventArgs());
            m_EventModule.SendEvent(this, new AnotherSimpleEventArgs());

            Assert.AreEqual(1, firstEventCount);
            Assert.AreEqual(0, secondEventCount);

            m_EventModule.Update(new TimeStruct(0f, 0f, 0f, 0f));

            Assert.AreEqual(3, firstEventCount);
            Assert.AreEqual(1, secondEventCount);

            m_EventModule.RemoveEventListener(OneSimpleEventArgs.TheEventId, onHearFirstEvent);
            m_EventModule.RemoveEventListener(AnotherSimpleEventArgs.TheEventId, onHearSecondEvent);

            m_EventModule.SendEvent(this, new OneSimpleEventArgs());
            m_EventModule.SendEvent(this, new AnotherSimpleEventArgs());

            m_EventModule.Update(new TimeStruct(0f, 0f, 0f, 0f));

            Assert.AreEqual(4, firstEventCount);
            Assert.AreEqual(1, secondEventCount);
        }

        [Test]
        public void TestSendEventNow()
        {
            int firstEventCount = 0;

            OnHearEvent onHearFirstEvent = (sender, e) =>
            {
                Assert.AreSame(sender, this);
                Assert.True(e is OneSimpleEventArgs);
                firstEventCount++;
            };

            m_EventModule.AddEventListener(OneSimpleEventArgs.TheEventId, onHearFirstEvent);
            m_EventModule.SendEventNow(this, new OneSimpleEventArgs());
            Assert.AreEqual(1, firstEventCount);
            m_EventModule.Update(new TimeStruct(0f, 0f, 0f, 0f));
            Assert.AreEqual(1, firstEventCount);
        }

        [Test]
        public void TestRemoveUnaddedListener()
        {
            OnHearEvent onHearFirstEvent = (sender, e) => { };
            m_EventModule.RemoveEventListener(OneSimpleEventArgs.TheEventId, onHearFirstEvent);
        }

        [Test]
        public void TestAddRemoveNullListener()
        {
            Assert.Throws<ArgumentNullException>(() => { m_EventModule.AddEventListener(OneSimpleEventArgs.TheEventId, null); });

            Assert.Throws<ArgumentNullException>(() => { m_EventModule.RemoveEventListener(OneSimpleEventArgs.TheEventId, null); });
        }

        [Test]
        public void TestInitBeforeSettingMainThreadId()
        {
            IEventModule anotherEventModule = new EventModule();
            Assert.Throws<InvalidOperationException>(() => anotherEventModule.Init());
        }

        [Test]
        public void TestUseWithoutInit()
        {
            IEventModule anotherEventModule = new EventModule();
            anotherEventModule.MainThreadId = Thread.CurrentThread.ManagedThreadId;
            Assert.Throws<InvalidOperationException>(() => anotherEventModule.SendEvent(null, new OneSimpleEventArgs()));
            Assert.Throws<InvalidOperationException>(() => anotherEventModule.ShutDown());
        }

        [Test]
        public void TestShutdownTwice()
        {
            IEventModule anotherEventModule = new EventModule();
            anotherEventModule.MainThreadId = Thread.CurrentThread.ManagedThreadId;
            anotherEventModule.Init();
            anotherEventModule.ShutDown();
            Assert.Throws<InvalidOperationException>(() => anotherEventModule.ShutDown());
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
                        m_EventModule.AddEventListener(OneSimpleEventArgs.TheEventId, (sender, e) => { });
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionsCaught++;
                    }

                    try
                    {
                        m_EventModule.RemoveEventListener(OneSimpleEventArgs.TheEventId, (sender, e) => { });
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionsCaught++;
                    }

                    try
                    {
                        m_EventModule.SendEventNow(null, new OneSimpleEventArgs());
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
                m_EventModule.AddEventListener(OneSimpleEventArgs.TheEventId, (sender, o) => { eventsReceived++; });
                var thread = new Thread(() =>
                {
                    try
                    {
                        m_EventModule.SendEvent(null, new OneSimpleEventArgs());
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionsCaught++;
                    }
                });

                thread.Start();
                thread.Join();
                Assert.AreEqual(0, eventsReceived);
                m_EventModule.Update(new TimeStruct(0f, 0f, 0f, 0f));
                Assert.AreEqual(1, eventsReceived);
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
                    m_EventModule.RemoveEventListener(OneSimpleEventArgs.TheEventId, eventListeners[index + 1]);
                    m_EventModule.SendEvent(null, new OneSimpleEventArgs());
                });
            }

            foreach (var eventListener in eventListeners)
            {
                m_EventModule.AddEventListener(OneSimpleEventArgs.TheEventId, eventListener);
            }

            m_EventModule.SendEventNow(null, new OneSimpleEventArgs());
            CollectionAssert.AreEqual(new[] {0, 1, 2, 3}, eventRecorder);
            m_EventModule.Update(new TimeStruct());
            CollectionAssert.AreEqual(new [] {0, 1, 2, 3, 0, 1, 3}, eventRecorder);
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
                    m_EventModule.RemoveEventListener(OneSimpleEventArgs.TheEventId, eventListeners[index + 1]);
                    m_EventModule.SendEventNow(null, new OneSimpleEventArgs());
                });
            }

            foreach (var eventListener in eventListeners)
            {
                m_EventModule.AddEventListener(OneSimpleEventArgs.TheEventId, eventListener);
            }

            m_EventModule.SendEventNow(null, new OneSimpleEventArgs());
            CollectionAssert.AreEqual(new[] {0, 1, 0, 1, 3, 2, 3}, eventRecorder);
        }

        [SetUp]
        public void SetUp()
        {
            Assert.IsNull(m_EventModule);
            m_EventModule = new EventModule();
            m_EventModule.MainThreadId = Thread.CurrentThread.ManagedThreadId;
            m_EventModule.Init();
        }

        [TearDown]
        public void TearDown()
        {
            Assert.IsNotNull(m_EventModule);
            m_EventModule.ShutDown();
            m_EventModule = null;
        }
    }
}
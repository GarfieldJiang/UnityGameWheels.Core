using System;
using System.Collections.Generic;
using COL.UnityGameWheels.Core.RedDot;
using NUnit.Framework;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class RedDotTests
    {
        private IRedDotService m_RedDotService;

        private class RedDotObserver : IRedDotObserver
        {
            private readonly Action<string, int> m_OnChange;

            public RedDotObserver(Action<string, int> onChange = null)
            {
                m_OnChange = onChange;
            }

            public void OnChange(string key, int value)
            {
                m_OnChange?.Invoke(key, value);
            }
        }

        [SetUp]
        public void SetUp()
        {
            m_RedDotService = new RedDotService();
        }

        [TearDown]
        public void TearDown()
        {
            m_RedDotService = null;
        }

        [Test]
        public void TestBasic()
        {
            var keys = new[]
            {
                "Leaf0", "Leaf1", "Leaf2", "NonLeaf0", "NonLeaf1", "Root",
            };
            m_RedDotService.AddLeaves(new[] {"Leaf0", "Leaf1", "Leaf2"});
            m_RedDotService.AddNonLeaf("NonLeaf0", NonLeafOperation.Sum, new[] {"Leaf0", "Leaf1"});
            m_RedDotService.AddNonLeaf("NonLeaf1", NonLeafOperation.Sum, new[] {"Leaf1", "Leaf2"});
            m_RedDotService.AddNonLeaf("Root", NonLeafOperation.Or, new[] {"NonLeaf0", "NonLeaf1"});
            m_RedDotService.SetUp();
            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, m_RedDotService.GetValue("Leaf0"));
                Assert.AreEqual(0, m_RedDotService.GetValue("Leaf1"));
                Assert.AreEqual(0, m_RedDotService.GetValue("Leaf2"));
                Assert.AreEqual(0, m_RedDotService.GetValue("NonLeaf0"));
                Assert.AreEqual(0, m_RedDotService.GetValue("NonLeaf1"));
                Assert.AreEqual(0, m_RedDotService.GetValue("Root"));
            });

            var valuesGotInOnChange = new Dictionary<string, int>();
            foreach (var key in keys)
            {
                valuesGotInOnChange[key] = 0;
            }

            var observer = new RedDotObserver((key, value) => valuesGotInOnChange[key] = value);
            foreach (var key in keys)
            {
                m_RedDotService.AddObserver(key, observer);
            }

            m_RedDotService.SetLeafValue("Leaf0", 1);
            m_RedDotService.SetLeafValue("Leaf1", 2);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, m_RedDotService.GetValue("NonLeaf0"));
                Assert.AreEqual(0, m_RedDotService.GetValue("NonLeaf1"));
                Assert.AreEqual(0, m_RedDotService.GetValue("Root"));
            });
            ((ITickable)m_RedDotService).OnUpdate(default(TimeStruct));
            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, m_RedDotService.GetValue("NonLeaf0"));
                Assert.AreEqual(3, valuesGotInOnChange["NonLeaf0"]);
                Assert.AreEqual(2, m_RedDotService.GetValue("NonLeaf1"));
                Assert.AreEqual(2, valuesGotInOnChange["NonLeaf1"]);
                Assert.AreEqual(1, m_RedDotService.GetValue("Root"));
                Assert.AreEqual(1, valuesGotInOnChange["Root"]);
            });

            m_RedDotService.RemoveObserver("Leaf0", observer);
            m_RedDotService.SetLeafValue("Leaf0", 4);
            ((ITickable)m_RedDotService).OnUpdate(default(TimeStruct));
            Assert.AreEqual(1, valuesGotInOnChange["Leaf0"]);
        }

        [Test]
        public void TestUseBeforeSetup()
        {
            const string LeafNodeName = "Leaf";
            m_RedDotService.AddLeaf(LeafNodeName);
            Assert.Throws<InvalidOperationException>(() => m_RedDotService.AddObserver(LeafNodeName, new RedDotObserver()));
        }

        [Test]
        public void TestAddNodeAfterSetup()
        {
            const string LeafNodeName = "Leaf";
            const string NonLeafNodeName = "NonLeaf";
            const string AnotherLeafNodeName = "AnotherLeaf";
            m_RedDotService.AddLeaf(LeafNodeName);
            m_RedDotService.SetUp();
            Assert.Throws<InvalidOperationException>(() => m_RedDotService.AddLeaf(AnotherLeafNodeName));
            Assert.Throws<InvalidOperationException>(() => m_RedDotService.AddNonLeaf(NonLeafNodeName,
                NonLeafOperation.Sum, new[] {LeafNodeName}));
        }

        [Test]
        public void TestAddNonLeafWithNoDependency()
        {
            const string LeafNodeName = "Leaf";
            const string NonLeafNodeName = "NonLeaf";
            m_RedDotService.AddLeaf(LeafNodeName);
            Assert.Throws<InvalidOperationException>(() =>
                m_RedDotService.AddNonLeaf(NonLeafNodeName, NonLeafOperation.Sum, new string[] { }));
        }

        [Test]
        public void TestLoopDependency()
        {
            const string LeafNodeName = "Leaf";
            const string NonLeafNodeName0 = "NonLeaf0";
            const string NonLeafNodeName1 = "NonLeaf1";
            const string NonLeafNodeName2 = "NonLeaf2";
            m_RedDotService.AddLeaf(LeafNodeName);
            m_RedDotService.AddNonLeaf(NonLeafNodeName0, NonLeafOperation.Sum, new[]
            {
                NonLeafNodeName1, LeafNodeName
            });

            m_RedDotService.AddNonLeaf(NonLeafNodeName1, NonLeafOperation.Sum, new[]
            {
                NonLeafNodeName2, LeafNodeName
            });

            m_RedDotService.AddNonLeaf(NonLeafNodeName2, NonLeafOperation.Sum, new[]
            {
                NonLeafNodeName0, LeafNodeName
            });

            Assert.Throws<InvalidOperationException>(() => m_RedDotService.SetUp());
        }

        [Test]
        public void TestNullOrEmptyNodeName()
        {
            Assert.Throws<ArgumentException>(() => m_RedDotService.AddLeaf(null));
            Assert.Throws<ArgumentException>(() =>
                m_RedDotService.AddNonLeaf(string.Empty, NonLeafOperation.Or, new[]
                {
                    "Anything"
                }));
        }

        [Test]
        public void TestGetValueOnAddingObserver()
        {
            const string LeafNodeName = "Leaf";
            m_RedDotService.AddLeaf(LeafNodeName);
            const string NonLeafNodeName = "NonLeaf";
            m_RedDotService.AddNonLeaf(NonLeafNodeName, NonLeafOperation.Sum, new[]
            {
                LeafNodeName
            });

            m_RedDotService.SetUp();
            m_RedDotService.SetLeafValue(LeafNodeName, 100);
            ((ITickable)m_RedDotService).OnUpdate(default(TimeStruct));
            int getValueInOnChange = 0;
            m_RedDotService.AddObserver(NonLeafNodeName, new RedDotObserver((key, value) => { getValueInOnChange = value; }));
            Assert.AreEqual(100, getValueInOnChange);
        }

        [Test]
        public void TestRemoveObserver()
        {
            const string LeafNodeName = "Leaf";
            m_RedDotService.AddLeaf(LeafNodeName);
            m_RedDotService.SetUp();
            int getValueInOnChange = 0;
            var observer = new RedDotObserver((key, value) => { getValueInOnChange = value; });
            m_RedDotService.AddObserver(LeafNodeName, observer);
            m_RedDotService.SetLeafValue(LeafNodeName, 100);
            ((ITickable)m_RedDotService).OnUpdate(default(TimeStruct));
            Assert.AreEqual(100, getValueInOnChange);
            m_RedDotService.RemoveObserver(LeafNodeName, observer);
            m_RedDotService.SetLeafValue(LeafNodeName, 90);
            ((ITickable)m_RedDotService).OnUpdate(default(TimeStruct));
            Assert.AreEqual(100, getValueInOnChange);
        }

        [Test]
        public void TestSetUpFlagAndEvent()
        {
            int onSetUpCallCount = 0;
            m_RedDotService.OnSetUp += () =>
            {
                Assert.True(m_RedDotService.IsSetUp);
                onSetUpCallCount++;
            };
            Assert.False(m_RedDotService.IsSetUp);
            Assert.Zero(onSetUpCallCount);
            m_RedDotService.SetUp();
            Assert.True(m_RedDotService.IsSetUp);
            Assert.AreEqual(1, onSetUpCallCount);
        }

        [Test]
        public void TestSetUpTwice()
        {
            m_RedDotService.SetUp();
            Assert.Throws<InvalidOperationException>(() => m_RedDotService.SetUp());
        }

        [Test]
        public void TestDependingOnSameKeyTwice()
        {
            m_RedDotService.AddLeaf("Leaf0");
            m_RedDotService.AddNonLeaf("NonLeaf0", NonLeafOperation.Sum, new[] {"Leaf0"});
            m_RedDotService.AddNonLeaf("NonLeaf1", NonLeafOperation.Sum, new[] {"Leaf0"});
            m_RedDotService.AddNonLeaf("Root", NonLeafOperation.Sum, new[] {"NonLeaf0", "NonLeaf1"});
            m_RedDotService.SetUp();
            m_RedDotService.SetLeafValue("Leaf0", 1);
            ((ITickable)m_RedDotService).OnUpdate(default(TimeStruct));
            Assert.AreEqual(2, m_RedDotService.GetValue("Root"));
        }
    }
}
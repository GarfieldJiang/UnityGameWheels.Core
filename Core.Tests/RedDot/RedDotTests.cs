using System;
using System.Collections.Generic;
using COL.UnityGameWheels.Core.RedDot;
using NUnit.Framework;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class RedDotTests
    {
        private IRedDotModule m_RedDotModule;

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
            m_RedDotModule = new RedDotModule();
        }

        [TearDown]
        public void TearDown()
        {
            m_RedDotModule = null;
        }

        [Test]
        public void Basic()
        {
            var keys = new[]
            {
                "Leaf0", "Leaf1", "Leaf2", "NonLeaf0", "NonLeaf1", "Root",
            };
            m_RedDotModule.AddLeaves(new[] {"Leaf0", "Leaf1", "Leaf2"});
            m_RedDotModule.AddNonLeaf("NonLeaf0", NonLeafOperation.Sum, new[] {"Leaf0", "Leaf1"});
            m_RedDotModule.AddNonLeaf("NonLeaf1", NonLeafOperation.Sum, new[] {"Leaf1", "Leaf2"});
            m_RedDotModule.AddNonLeaf("Root", NonLeafOperation.Or, new[] {"NonLeaf0", "NonLeaf1"});
            m_RedDotModule.SetUp();
            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, m_RedDotModule.GetValue("Leaf0"));
                Assert.AreEqual(0, m_RedDotModule.GetValue("Leaf1"));
                Assert.AreEqual(0, m_RedDotModule.GetValue("Leaf2"));
                Assert.AreEqual(0, m_RedDotModule.GetValue("NonLeaf0"));
                Assert.AreEqual(0, m_RedDotModule.GetValue("NonLeaf1"));
                Assert.AreEqual(0, m_RedDotModule.GetValue("Root"));
            });

            var valuesGotInOnChange = new Dictionary<string, int>();
            foreach (var key in keys)
            {
                valuesGotInOnChange[key] = 0;
            }

            var observer = new RedDotObserver((key, value) => valuesGotInOnChange[key] = value);
            foreach (var key in keys)
            {
                m_RedDotModule.AddObserver(key, observer);
            }

            m_RedDotModule.SetLeafValue("Leaf0", 1);
            m_RedDotModule.SetLeafValue("Leaf1", 2);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, m_RedDotModule.GetValue("NonLeaf0"));
                Assert.AreEqual(0, m_RedDotModule.GetValue("NonLeaf1"));
                Assert.AreEqual(0, m_RedDotModule.GetValue("Root"));
            });
            m_RedDotModule.Update(default(TimeStruct));
            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, m_RedDotModule.GetValue("NonLeaf0"));
                Assert.AreEqual(3, valuesGotInOnChange["NonLeaf0"]);
                Assert.AreEqual(2, m_RedDotModule.GetValue("NonLeaf1"));
                Assert.AreEqual(2, valuesGotInOnChange["NonLeaf1"]);
                Assert.AreEqual(1, m_RedDotModule.GetValue("Root"));
                Assert.AreEqual(1, valuesGotInOnChange["Root"]);
            });
        }

        [Test]
        public void UseBeforeSetup()
        {
            const string LeafNodeName = "Leaf";
            m_RedDotModule.AddLeaf(LeafNodeName);
            Assert.Throws<InvalidOperationException>(() => m_RedDotModule.AddObserver(LeafNodeName, new RedDotObserver()));
        }

        [Test]
        public void AddNodeAfterSetup()
        {
            const string LeafNodeName = "Leaf";
            m_RedDotModule.SetUp();
            Assert.Throws<InvalidOperationException>(() => m_RedDotModule.AddLeaf(LeafNodeName));
        }

        [Test]
        public void AddNonLeafWithNoDependency()
        {
            const string LeafNodeName = "Leaf";
            const string NonLeafNodeName = "NonLeaf";
            m_RedDotModule.AddLeaf(LeafNodeName);
            Assert.Throws<InvalidOperationException>(() =>
                m_RedDotModule.AddNonLeaf(NonLeafNodeName, NonLeafOperation.Sum, new string[] { }));
        }

        [Test]
        public void LoopDependency()
        {
            const string LeafNodeName = "Leaf";
            const string NonLeafNodeName0 = "NonLeaf0";
            const string NonLeafNodeName1 = "NonLeaf1";
            const string NonLeafNodeName2 = "NonLeaf2";
            m_RedDotModule.AddLeaf(LeafNodeName);
            m_RedDotModule.AddNonLeaf(NonLeafNodeName0, NonLeafOperation.Sum, new[] {NonLeafNodeName1, LeafNodeName});
            m_RedDotModule.AddNonLeaf(NonLeafNodeName1, NonLeafOperation.Sum, new[] {NonLeafNodeName2, LeafNodeName});
            m_RedDotModule.AddNonLeaf(NonLeafNodeName2, NonLeafOperation.Sum, new[] {NonLeafNodeName0, LeafNodeName});
            Assert.Throws<InvalidOperationException>(() => m_RedDotModule.SetUp());
        }

        [Test]
        public void NullOrEmptyNodeName()
        {
            Assert.Throws<ArgumentException>(() => m_RedDotModule.AddLeaf(null));
            Assert.Throws<ArgumentException>(() =>
                m_RedDotModule.AddNonLeaf(string.Empty, NonLeafOperation.Or, new[] {"Anything"}));
        }
    }
}
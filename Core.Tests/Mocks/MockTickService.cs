using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.Tests
{
    public class MockTickService : ITickService
    {
        private class Node
        {
            internal int order;
            internal Action<TimeStruct> callback;
        }

        private readonly List<Node> m_UpdateCallbacks = new List<Node>();
        private readonly List<Node> m_LateUpdateCallbacks = new List<Node>();

        public void ManualUpdate(TimeStruct timeStruct)
        {
            foreach (var node in m_UpdateCallbacks)
            {
                node.callback(timeStruct);
            }
        }

        public void AddUpdateCallback(Action<TimeStruct> updateCallback, int order)
        {
            m_UpdateCallbacks.Add(new Node {callback = updateCallback, order = order});
            m_UpdateCallbacks.Sort(CompareNodeOrder);
        }

        public bool RemoveUpdateCallback(Action<TimeStruct> updateFunc)
        {
            return m_UpdateCallbacks.RemoveAll(node => node.callback == updateFunc) > 0;
        }

        public void AddLateUpdateCallback(Action<TimeStruct> lateUpdateFunc, int order)
        {
            throw new NotImplementedException();
        }

        public bool RemoveLateUpdateCallback(Action<TimeStruct> lateUpdateFunc)
        {
            throw new NotImplementedException();
        }

        private static int CompareNodeOrder(Node x, Node y)
        {
            return x.order.CompareTo(y.order);
        }
    }
}
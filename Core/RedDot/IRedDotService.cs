using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.RedDot
{
    public interface IRedDotService : ILifeCycle
    {
        bool IsSetUp { get; }
        event Action OnSetUp;
        void AddLeaf(string key);
        void AddLeaves(IEnumerable<string> key);
        void AddNonLeaf(string key, NonLeafOperation operation, IEnumerable<string> dependencies);
        void SetUp();
        void SetLeafValue(string key, int value);
        int GetValue(string key);
        void AddObserver(string key, IRedDotObserver observer);
        bool RemoveObserver(string key, IRedDotObserver observer);
    }
}
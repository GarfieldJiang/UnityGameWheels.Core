using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.RedDot
{
    public partial class RedDotModule
    {
        private abstract class Node
        {
            public string Key;
            public int Value = 0;
            public readonly HashSet<string> ReverseDependencies = new HashSet<string>();
            public List<IRedDotObserver> Observers = null;
            public abstract RedDotNodeType Type { get; }
        }
    }
}
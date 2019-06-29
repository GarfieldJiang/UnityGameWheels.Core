using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.RedDot
{
    public partial class RedDotModule
    {
        private abstract class Node
        {
            public string Key;
            public int Value = 0;
            public readonly HashSet<string> BeDependedOn = new HashSet<string>();
            public List<IRedDotObserver> Observers = null;
        }
    }
}
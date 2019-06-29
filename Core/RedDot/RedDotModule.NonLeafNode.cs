using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.RedDot
{
    public partial class RedDotModule
    {
        private class NonLeafNode : Node
        {
            public NonLeafOperation Operation;
            public readonly HashSet<string> DependsOn = new HashSet<string>();
        }
    }
}
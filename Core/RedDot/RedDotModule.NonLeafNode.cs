using System.Collections.Generic;

namespace COL.UnityGameWheels.Core.RedDot
{
    public partial class RedDotService
    {
        private class NonLeafNode : Node
        {
            public override RedDotNodeType Type => RedDotNodeType.NonLeaf;
            public NonLeafOperation Operation;
            public readonly HashSet<string> Dependencies = new HashSet<string>();
        }
    }
}
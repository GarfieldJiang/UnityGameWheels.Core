namespace COL.UnityGameWheels.Core.RedDot
{
    public partial class RedDotModule
    {
        private class LeafNode : Node
        {
            public override RedDotNodeType Type => RedDotNodeType.Leaf;
        }
    }
}
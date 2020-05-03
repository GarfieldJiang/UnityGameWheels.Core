namespace COL.UnityGameWheels.Core.RedDot
{
    public partial class RedDotService
    {
        private class LeafNode : Node
        {
            public override RedDotNodeType Type => RedDotNodeType.Leaf;
        }
    }
}
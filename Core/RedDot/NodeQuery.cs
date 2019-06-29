namespace COL.UnityGameWheels.Core.RedDot
{
    public class NodeQuery
    {
        public string Key { get; internal set; }
        public int Value { get; internal set; }
        public bool IsLeaf { get; internal set; }
        public NonLeafOperation Opeartion { get; internal set; }
        public string[] Dependencies { get; internal set; }
        public string[] ReverseDependencies { get; internal set; }
    }
}
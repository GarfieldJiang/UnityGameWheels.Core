using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace COL.UnityGameWheels.Core.Tests
{
    [TestFixture]
    public class TarjanSccTests
    {
        private class GraphVertex
        {
            public int Key;
            public HashSet<int> Successors = new HashSet<int>();
        }

        [Test]
        public void EmptyGraph()
        {
            var vertices = new Dictionary<int, GraphVertex>();
            var sccs = Algorithm.Graph.TarjanScc(vertices, (x, y) => x == y, v => v.Successors, false);
            Assert.Zero(sccs.Count);

            sccs = Algorithm.Graph.TarjanScc(vertices, (x, y) => x == y, v => v.Successors, true);
            Assert.Zero(sccs.Count);
        }

        [Test]
        public void OneVertex()
        {
            var vertices = new Dictionary<int, GraphVertex>();
            vertices.Add(1, new GraphVertex { Key = 1 });

            var sccs = Algorithm.Graph.TarjanScc(vertices, (x, y) => x == y, v => v.Successors, false);
            Assert.Zero(sccs.Count);

            sccs = Algorithm.Graph.TarjanScc(vertices, (x, y) => x == y, v => v.Successors, true);
            AssertSccResultAreEqual(new List<IList<int>> { new List<int> { 1 } }, sccs);

            vertices[1].Successors.Add(1);
            sccs = Algorithm.Graph.TarjanScc(vertices, (x, y) => x == y, v => v.Successors, false);
            AssertSccResultAreEqual(new List<IList<int>> { new List<int> { 1 } }, sccs);
        }

        [Test]
        public void NormalCase()
        {
            var vertices = new Dictionary<int, GraphVertex>
            {
                { 1, new GraphVertex{ Key = 1, Successors = new HashSet<int> { 2 } } },
                { 2, new GraphVertex{ Key = 2, Successors = new HashSet<int> { 3 } } },
                { 3, new GraphVertex{ Key = 3, Successors = new HashSet<int> { 1 } } },
                { 4, new GraphVertex{ Key = 4, Successors = new HashSet<int> { 2, 3, 5 } } },
                { 5, new GraphVertex{ Key = 5, Successors = new HashSet<int> { 4, 6 } } },
                { 6, new GraphVertex{ Key = 6, Successors = new HashSet<int> { 3, 7 } } },
                { 7, new GraphVertex{ Key = 7, Successors = new HashSet<int> { 6 } } },
                { 8, new GraphVertex{ Key = 8, Successors = new HashSet<int> { 5, 7, 8 } } },
            };

            var sccs = Algorithm.Graph.TarjanScc(vertices, (x, y) => x == y, v => v.Successors, true);
            var sccs2 = Algorithm.Graph.TarjanScc(vertices, (x, y) => x == y, v => v.Successors, false);
            var expected = new List<IList<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5 },
                new List<int> { 6, 7 },
                new List<int> { 8 },
            };

            AssertSccResultAreEqual(expected, sccs);
            AssertSccResultAreEqual(expected, sccs2);
        }

        private static void AssertSccResultAreEqual(IList<IList<int>> x, IList<IList<int>> y)
        {
            Assert.AreEqual(x.Count, y.Count);
            var xNew = new List<IList<int>>(x);
            var yNew = new List<IList<int>>(y);
            xNew.Sort((a, b) => a.Min().CompareTo(b.Min()));
            yNew.Sort((a, b) => a.Min().CompareTo(b.Min()));
            for (int i = 0; i < x.Count; i++)
            {
                CollectionAssert.AreEquivalent(xNew[i], yNew[i]);
            }
        }
    }
}
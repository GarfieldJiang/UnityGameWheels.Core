using System;
using System.Collections.Generic;

namespace COL.UnityGameWheels.Core
{
    public static partial class Algorithm
    {
        public static partial class Graph
        {
            /// <summary>
            /// Tarjan's algorithm to get strongly connected components.
            /// </summary>
            /// <typeparam name="K">Vertices key type.</typeparam>
            /// <typeparam name="V">Vertex type.</typeparam>
            /// <param name="vertices">Vertices.</param>
            /// <param name="keyEqualFunc">Delegate to check equality of keys.</param>
            /// <param name="getSuccessorKeysFunc">Delegate used to get successors of a given vertex.</param>
            /// <param name="includeSingleVertices">Whether the calculation should include single vertices.</param>
            /// <param name="sccs">Result strongly connected components.</param>
            public static void TarjanScc<K, V>(IDictionary<K, V> vertices,
                Func<K, K, bool> keyEqualFunc,
                Func<V, IEnumerable<K>> getSuccessorKeysFunc,
                bool includeSingleVertices, List<IList<K>> sccs)
            {
                if (vertices == null)
                {
                    throw new ArgumentNullException(nameof(vertices));
                }

                if (getSuccessorKeysFunc == null)
                {
                    throw new ArgumentNullException(nameof(getSuccessorKeysFunc));
                }

                if (sccs == null)
                {
                    throw new ArgumentNullException(nameof(sccs));
                }

                sccs.Clear();
                new TarjanSccExecutor<K, V>(vertices, keyEqualFunc ?? ((x, y) => Equals(x, y)), getSuccessorKeysFunc, includeSingleVertices, sccs).Execute();
            }

            /// <summary>
            /// Tarjan's algorithm to get strongly connected components.
            /// </summary>
            /// <typeparam name="K">Vertices key type.</typeparam>
            /// <typeparam name="V">Vertex type.</typeparam>
            /// <param name="vertices">Vertices.</param>
            /// <param name="keyEqualFunc">Delegate to check equality of keys.</param>
            /// <param name="getSuccessorKeysFunc">Delegate used to get successors of a given vertex.</param>
            /// <param name="includeSingleVertices">Whether the calculation should include single vertices.</param>
            /// <returns>Result strongly connected components.</returns>
            public static IList<IList<K>> TarjanScc<K, V>(IDictionary<K, V> vertices,
                Func<K, K, bool> keyEqualFunc,
                Func<V, IEnumerable<K>> getSuccessorKeysFunc,
                bool includeSingleVertices)
            {
                if (vertices == null)
                {
                    throw new ArgumentNullException(nameof(vertices));
                }

                if (getSuccessorKeysFunc == null)
                {
                    throw new ArgumentNullException(nameof(getSuccessorKeysFunc));
                }

                var sccs = new List<IList<K>>();
                new TarjanSccExecutor<K, V>(vertices, keyEqualFunc ?? ((x, y) => Equals(x, y)), getSuccessorKeysFunc, includeSingleVertices, sccs).Execute();
                return sccs;
            }
        }
    }
}
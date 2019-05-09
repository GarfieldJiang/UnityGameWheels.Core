using System;
using System.Collections.Generic;
using System.Linq;

namespace COL.UnityGameWheels.Core
{
    public static partial class Algorithm
    {
        public static partial class Graph
        {
            private class TarjanSccExecutor<K, V>
            {
                private IDictionary<K, V> m_Vertices;
                private Func<K, K, bool> m_KeyEqualFunc;
                private Func<V, IEnumerable<K>> m_GetSuccessorKeysFunc;
                private bool m_IncludeSingleVertices;
                private List<IList<K>> m_Sccs;

                private Dictionary<K, int> m_Indices = new Dictionary<K, int>();
                private Dictionary<K, int> m_LowLinks = new Dictionary<K, int>();
                private int m_Index = 0;
                private Stack<K> m_Stack = new Stack<K>();
                private HashSet<K> m_StackKeys = new HashSet<K>();

                public TarjanSccExecutor(IDictionary<K, V> vertices,
                    Func<K, K, bool> keyEqualFunc,
                    Func<V, IEnumerable<K>> getSuccessorKeysFunc,
                    bool includeSingleVertices, List<IList<K>> sccs)
                {
                    m_Vertices = vertices;
                    m_KeyEqualFunc = keyEqualFunc;
                    m_GetSuccessorKeysFunc = getSuccessorKeysFunc;
                    m_IncludeSingleVertices = includeSingleVertices;
                    m_Sccs = sccs;
                }

                public void Execute()
                {
                    foreach (var kv in m_Vertices)
                    {
                        if (m_Indices.ContainsKey(kv.Key))
                        {
                            continue;
                        }

                        Recur(kv.Key);
                    }
                }

                private void Recur(K key)
                {
                    m_Indices[key] = m_LowLinks[key] = m_Index;
                    ++m_Index;
                    m_Stack.Push(key);
                    m_StackKeys.Add(key);

                    var vertex = m_Vertices[key];
                    var successorKeys = m_GetSuccessorKeysFunc(vertex);
                    if (successorKeys != null)
                    {
                        foreach (var successorKey in successorKeys)
                        {
                            if (!m_Indices.TryGetValue(successorKey, out _))
                            {
                                Recur(successorKey);
                                m_LowLinks[key] = Math.Min(m_LowLinks[key], m_LowLinks[successorKey]);
                            }
                            else if (m_StackKeys.Contains(successorKey))
                            {
                                m_LowLinks[key] = Math.Min(m_LowLinks[key], m_Indices[successorKey]);
                            }
                        }
                    }

                    if (m_LowLinks[key] == m_Indices[key])
                    {
                        List<K> scc = new List<K>();
                        K another;
                        do
                        {
                            another = m_Stack.Pop();
                            m_StackKeys.Remove(another);
                            scc.Add(another);
                        }
                        while (!m_KeyEqualFunc(another, key));

                        if (scc.Count > 1)
                        {
                            m_Sccs.Add(scc);
                        }
                        else if (scc.Count == 1)
                        {
                            if (m_IncludeSingleVertices || m_GetSuccessorKeysFunc(m_Vertices[key]).Contains(scc[0]))
                            {
                                m_Sccs.Add(scc);
                            }
                        }
                    }
                }
            }
        }
    }
}
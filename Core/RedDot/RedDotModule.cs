using System;
using System.Collections.Generic;
using System.Text;

namespace COL.UnityGameWheels.Core.RedDot
{
    public partial class RedDotModule : BaseModule, IRedDotModule
    {
        private readonly Dictionary<string, LeafNode> m_LeafNodes = new Dictionary<string, LeafNode>();
        private readonly Dictionary<string, Node> m_Nodes = new Dictionary<string, Node>();
        private readonly Dictionary<string, NonLeafNode> m_NonLeafNodes = new Dictionary<string, NonLeafNode>();
        private readonly HashSet<string> m_LeafKeysWithModifiedValue = new HashSet<string>();
        private readonly HashSet<string> m_KeysNeedingRecalc = new HashSet<string>();
        private readonly HashSet<string> m_KeysNeedingRecalcCopied = new HashSet<string>();
        private readonly List<IRedDotObserver> m_ObserverListCopied = new List<IRedDotObserver>();

        private bool m_SetUp = false;

        public void AddLeaf(string key)
        {
            GuardNotSetUp();
            GuardKey(key, nameof(key));
            GuardKeyNotExist(key);
            DoAddLeaf(key);
        }

        private void DoAddLeaf(string key)
        {
            var node = new LeafNode {Key = key};
            m_Nodes.Add(key, node);
            m_LeafNodes.Add(key, node);
        }

        public void AddLeaves(IEnumerable<string> keys)
        {
            GuardNotSetUp();
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            foreach (var key in keys)
            {
                GuardKey(key, nameof(keys));
                GuardKeyNotExist(key);
                DoAddLeaf(key);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks>Debug use.</remarks>
        public NodeQuery GetNodeQuery(string key)
        {
            if (!m_Nodes.TryGetValue(key, out var node))
            {
                return null;
            }

            bool isLeaf = false;
            string[] dependencies = { };
            string[] reverseDependencies = new List<string>(node.BeDependedOn).ToArray();
            NonLeafOperation operation = (NonLeafOperation)(-1);
            if (node is LeafNode leafNode)
            {
                isLeaf = true;
            }
            else if (node is NonLeafNode nonLeafNode)
            {
                dependencies = new List<string>(nonLeafNode.DependsOn).ToArray();
                operation = nonLeafNode.Operation;
            }

            return new NodeQuery
            {
                Key = key,
                Value = node.Value,
                IsLeaf = isLeaf,
                Dependencies = dependencies,
                ReverseDependencies = reverseDependencies,
                Opeartion = operation,
            };
        }

        public void AddNonLeaf(string key, NonLeafOperation operation, IEnumerable<string> dependsOn)
        {
            GuardNotSetUp();
            GuardKey(key, nameof(key));
            GuardKeyNotExist(key);
            GuardOperation(operation);
            if (dependsOn == null)
            {
                throw new ArgumentNullException(nameof(dependsOn));
            }

            int dependCount = 0;
            var tempSet = new HashSet<string>(dependsOn);
            foreach (var dependency in tempSet)
            {
                GuardKey(dependency, nameof(dependsOn));
                dependCount++;
            }

            if (dependCount == 0)
            {
                throw new InvalidOperationException("Non-leaf node must depends on something");
            }

            var node = new NonLeafNode {Key = key, Operation = operation};
            node.DependsOn.UnionWith(tempSet);
            m_Nodes.Add(key, node);
            m_NonLeafNodes.Add(key, node);
        }

        private void CheckDependencyExistenceOrThrow()
        {
            foreach (var node in m_Nodes.Values)
            {
                if (!(node is NonLeafNode nonLeafNode))
                {
                    continue;
                }

                foreach (var dependencyKey in nonLeafNode.DependsOn)
                {
                    if (!m_Nodes.ContainsKey(dependencyKey))
                    {
                        throw new InvalidOperationException($"Key [{dependencyKey}] in [{nonLeafNode.Key}]'s dependencies doesn't exist.");
                    }
                }
            }
        }

        private void CheckNoLoopOrThrow()
        {
            var emptyEnumerable = new string[] { };
            var sccs = Algorithm.Graph.TarjanScc(m_Nodes, (x, y) => x == y, node =>
            {
                if (node is NonLeafNode nonLeafNode)
                {
                    return nonLeafNode.DependsOn;
                }

                return emptyEnumerable;
            }, false);

            if (sccs.Count <= 0)
            {
                return;
            }

            var sb = new StringBuilder();

            var scc = sccs[0];
            sb.Append("[");
            bool first = true;
            foreach (var key in scc)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append(key);
            }

            sb.Append("]");
            throw new InvalidOperationException($"Loop dependency detected. One strongly connected components shown as follows:\n{sb}");
        }

        private void BuildReverseDependency()
        {
            foreach (var nonLeafNode in m_NonLeafNodes.Values)
            {
                foreach (var dependencyKey in nonLeafNode.DependsOn)
                {
                    m_Nodes[dependencyKey].BeDependedOn.Add(nonLeafNode.Key);
                }
            }
        }

        public void SetUp()
        {
            CheckDependencyExistenceOrThrow();
            CheckNoLoopOrThrow();
            BuildReverseDependency();
            m_SetUp = true;
        }

        public void SetLeafValue(string key, int value)
        {
            GuardSetUp();
            GuardKey(key, nameof(key));
            if (!m_LeafNodes.TryGetValue(key, out var leafNode))
            {
                throw new InvalidOperationException($"There is no leaf with key [{key}].");
            }

            if (leafNode.Value == value)
            {
                return;
            }

            leafNode.Value = value;
            m_LeafKeysWithModifiedValue.Add(key);
        }

        public int GetValue(string key)
        {
            GuardSetUp();
            GuardKey(key, nameof(key));
            if (!m_Nodes.TryGetValue(key, out var node))
            {
                throw new InvalidOperationException($"There is no node with key [{key}].");
            }

            return node.Value;
        }

        public void AddObserver(string key, IRedDotObserver observer)
        {
            GuardSetUp();
            GuardKey(key, nameof(key));
            if (!m_Nodes.TryGetValue(key, out var node))
            {
                throw new InvalidOperationException($"There is no node with key [{key}].");
            }

            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (node.Observers == null)
            {
                node.Observers = new List<IRedDotObserver>();
            }

            node.Observers.Add(observer);
            observer.OnChange(key, node.Value);
        }

        public bool RemoveObserver(string key, IRedDotObserver observer)
        {
            GuardSetUp();
            GuardKey(key, nameof(key));
            if (!m_Nodes.TryGetValue(key, out var node))
            {
                throw new InvalidOperationException($"There is no node with key [{key}].");
            }

            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (node.Observers == null)
            {
                return false;
            }

            return node.Observers.Remove(observer);
        }

        public override void Update(TimeStruct timeStruct)
        {
            if (m_LeafKeysWithModifiedValue.Count <= 0)
            {
                return;
            }

            foreach (var key in m_LeafKeysWithModifiedValue)
            {
                Notify(m_LeafNodes[key]);
                SetNeedRecalcFrom(key);
            }

            m_KeysNeedingRecalcCopied.UnionWith(m_KeysNeedingRecalc);
            foreach (var key in m_KeysNeedingRecalcCopied)
            {
                RecalcValueAndNotify(key);
            }

            m_LeafKeysWithModifiedValue.Clear();
            m_KeysNeedingRecalc.Clear();
            m_KeysNeedingRecalcCopied.Clear();
        }

        private void RecalcValueAndNotify(string key)
        {
            if (!m_KeysNeedingRecalc.Contains(key))
            {
                return;
            }

            m_KeysNeedingRecalc.Remove(key);
            var node = m_NonLeafNodes[key];
            foreach (var dependOnKey in node.DependsOn)
            {
                RecalcValueAndNotify(dependOnKey);
            }

            int newValue = 0;
            switch (node.Operation)
            {
                case NonLeafOperation.Sum:
                    newValue = Sum(node.DependsOn);
                    break;
                case NonLeafOperation.Or:
                    newValue = Or(node.DependsOn);
                    break;
            }

            if (newValue != node.Value)
            {
                node.Value = newValue;
                Notify(node);
            }
        }

        private void Notify(Node node)
        {
            if (node.Observers == null)
            {
                return;
            }

            m_ObserverListCopied.AddRange(node.Observers);
            try
            {
                foreach (var observer in m_ObserverListCopied)
                {
                    observer.OnChange(node.Key, node.Value);
                }
            }
            finally
            {
                m_ObserverListCopied.Clear();
            }
        }

        private int Or(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (m_Nodes[key].Value != 0)
                {
                    return 1;
                }
            }

            return 0;
        }

        private int Sum(IEnumerable<string> keys)
        {
            var result = 0;
            foreach (var key in keys)
            {
                result += m_Nodes[key].Value;
            }

            return result;
        }

        private void SetNeedRecalcFrom(string key)
        {
            var node = m_Nodes[key];
            foreach (var reverseDependencyKey in node.BeDependedOn)
            {
                if (m_KeysNeedingRecalc.Contains(reverseDependencyKey))
                {
                    continue;
                }

                m_KeysNeedingRecalc.Add(reverseDependencyKey);
                SetNeedRecalcFrom(reverseDependencyKey);
            }
        }

        private void GuardKey(string key, string name)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Invalid key.", name);
            }
        }

        private void GuardKeyNotExist(string key)
        {
            if (m_Nodes.ContainsKey(key))
            {
                throw new ArgumentException($"Key [{key}] already exists.");
            }
        }

        private void GuardOperation(NonLeafOperation operation)
        {
            if (operation != NonLeafOperation.Or && operation != NonLeafOperation.Sum)
            {
                throw new ArgumentException($"Unknown non-leaf operation [{operation}].");
            }
        }

        private void GuardNotSetUp()
        {
            if (m_SetUp)
            {
                throw new InvalidOperationException("Cannot be done after setting up.");
            }
        }

        private void GuardSetUp()
        {
            if (!m_SetUp)
            {
                throw new InvalidOperationException("Cannot be done before setting up.");
            }
        }
    }
}
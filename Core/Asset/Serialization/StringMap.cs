using System;
using System.Collections.Generic;
using System.IO;

namespace COL.UnityGameWheels.Core.Asset
{
    internal class StringMap : IBinarySerializable
    {
        private readonly List<string> m_StringList;
        private readonly Dictionary<string, int> m_Map;
        private int m_CurrentId;

        internal StringMap(int capacity = 4096)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            m_StringList = new List<string>(capacity);
            m_Map = new Dictionary<string, int>(capacity);
        }

        internal bool TryAddString(string str, out int id)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            id = -1;
            if (m_Map.ContainsKey(str))
            {
                return false;
            }

            id = m_CurrentId++;
            m_Map[str] = id;
            m_StringList.Add(str);
            return true;
        }

        internal int Count => m_StringList.Count;

        internal string GetString(int id)
        {
            return m_StringList[id];
        }

        internal int GetId(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return m_Map[str];
        }

        public void ToBinary(BinaryWriter bw)
        {
            bw.Write(m_StringList.Count);
            foreach (var str in m_StringList)
            {
                bw.Write(str);
            }
        }

        public void FromBinary(BinaryReader br)
        {
            var count = br.ReadInt32();
            m_StringList.Clear();
            m_Map.Clear();
            for (int id = 0; id < count; id++)
            {
                var str = br.ReadString();
                m_StringList.Add(str);
                m_Map[str] = id;
            }
        }
    }
}
using System;

namespace COL.UnityGameWheels.Core
{
    public sealed class Version : IComparable, IComparable<Version>, IEquatable<Version>, ICloneable
    {
        private readonly int m_Major;
        private readonly int m_Minor;
        private readonly int m_Patch;

        public int Major => m_Major;
        public int Minor => m_Minor;
        public int Patch => m_Patch;

        public Version()
        {
            m_Major = m_Minor = m_Patch = 0;
        }

        public Version(int major, int minor, int patch)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major));
            }

            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor));
            }

            if (patch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patch));
            }

            m_Major = major;
            m_Minor = minor;
            m_Patch = patch;
        }

        public Version(string versionText)
        {
            if (string.IsNullOrWhiteSpace(versionText))
            {
                throw new ArgumentException($"Invalid '{nameof(versionText)}'.");
            }

            var segments = versionText.Split('.');
            if (segments.Length > 3)
            {
                throw new ArgumentException($"Version can have at most 3 segments.");
            }

            if (!int.TryParse(segments[0], out var major) || major < 0)
            {
                throw new ArgumentException($"Illegal major version.");
            }

            var minor = 0;
            if (segments.Length > 1 && (!int.TryParse(segments[1], out minor) || minor < 0))
            {
                throw new ArgumentException($"Illegal minor version.");
            }

            var patch = 0;
            if (segments.Length > 2 && (!int.TryParse(segments[2], out patch) || patch < 0))
            {
                throw new ArgumentException($"Illegal patch version.");
            }

            m_Major = major;
            m_Minor = minor;
            m_Patch = patch;
        }

        public object Clone()
        {
            return new Version(m_Major, m_Minor, m_Patch);
        }

        public int CompareTo(Version other)
        {
            var majorComparison = m_Major.CompareTo(other.m_Major);
            if (majorComparison != 0)
            {
                return majorComparison;
            }

            var minorComparison = m_Minor.CompareTo(other.m_Minor);
            return minorComparison != 0 ? minorComparison : m_Patch.CompareTo(other.m_Patch);
        }

        public bool Equals(Version other)
        {
            if (other == null)
            {
                return false;
            }
            return m_Major == other.m_Major && m_Minor == other.m_Minor && m_Patch == other.m_Patch;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Version other))
            {
                return false;
            }

            return Equals(other);
        }

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is Version other))
            {
                throw new ArgumentException($"Invalid type for '{nameof(obj)}'.");
            }

            return CompareTo(other);
        }

        public override string ToString()
        {
            return ToString(3);
        }

        public string ToString(int leastSegmentCount)
        {
            if (leastSegmentCount <= 0 || leastSegmentCount >= 4)
            {
                throw new ArgumentOutOfRangeException(nameof(leastSegmentCount), "Version has 1 to 3 segments.");
            }

            var sb = StringBuilderCache.Acquire();
            sb.Append(m_Major);
            if (m_Minor == 0 && m_Patch == 0 && leastSegmentCount <= 1)
            {
                return StringBuilderCache.GetStringAndRelease(sb);
            }

            sb.Append('.');
            sb.Append(m_Minor);
            if (m_Patch == 0 && leastSegmentCount <= 2)
            {
                return StringBuilderCache.GetStringAndRelease(sb);
            }

            sb.Append('.');
            sb.Append(m_Patch);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public override int GetHashCode()
        {
            var hashCode = 248502537;
            hashCode = hashCode * -1521134295 + m_Major.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Minor.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Patch.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Version v1, Version v2)
        {
            return v1?.Equals(v2) ?? ReferenceEquals(v2, null);
        }

        public static bool operator !=(Version v1, Version v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(Version v1, Version v2)
        {
            if ((object)v1 == null)
            {
                throw new ArgumentNullException(nameof(v1));
            }

            return v1.CompareTo(v2) < 0;
        }

        public static bool operator <=(Version v1, Version v2)
        {
            if ((object)v1 == null)
            {
                throw new ArgumentNullException(nameof(v1));
            }

            return v1.CompareTo(v2) <= 0;
        }

        public static bool operator >(Version v1, Version v2)
        {
            return v2 < v1;
        }

        public static bool operator >=(Version v1, Version v2)
        {
            return v2 <= v1;
        }
    }
}
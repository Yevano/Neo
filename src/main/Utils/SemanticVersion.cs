using System;

namespace Neo.Utils {
    public struct SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion> {
        public SemanticVersion(byte major, byte minor, byte patch) {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public byte Major { get; }
        public byte Minor { get; }
        public byte Patch { get; }

        public int CompareTo(SemanticVersion other) {
            if (other.Major > Major) {
                return -1;
            } else if (Major > other.Major) {
                return 1;
            }

            if (other.Minor > Minor) {
                return -1;
            } else if (Minor > other.Minor) {
                return 1;
            }

            if (other.Patch > Patch) {
                return -1;
            } else if (Patch > other.Patch) {
                return 1;
            }

            return 0;
        }

        public static bool operator ==(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) == 0;
        public static bool operator !=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) != 0;
        public static bool operator >(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) == 1;
        public static bool operator <(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) == -1;
        public static bool operator >=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) >= 0;
        public static bool operator <=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) <= 0;

        public bool Equals(SemanticVersion other) => Major == other.Major && Minor == other.Minor && Patch == other.Patch;

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            var other = (SemanticVersion)obj;
            return Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hash = Major.GetHashCode();
                hash = 31 * hash + Minor.GetHashCode();
                hash = 31 * hash + Patch.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"{Major}.{Minor}.{Patch}";

        public static SemanticVersion Parse(string v) {
            if (!TryParse(v, out SemanticVersion version)) {
                throw new ArgumentException($"Malformed semantic version: '{v}'");
            }
            return version;
        }

        public static bool TryParse(string v, out SemanticVersion result) {
            var parts = v.Split('.');

            if (parts.Length != 3) {
                throw new ArgumentException($"Malformed semantic version: '{v}'");
            }

            if (!byte.TryParse(parts[0], out var major)) {
                result = default(SemanticVersion);
                return false;
            }

            if (!byte.TryParse(parts[1], out var minor)) {
                result = default(SemanticVersion);
                return false;
            }

            if (!byte.TryParse(parts[2], out var patch)) {
                result = default(SemanticVersion);
                return false;
            }

            result = new SemanticVersion(major, minor, patch);
            return true;
        }
    }
}
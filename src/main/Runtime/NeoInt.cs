namespace Neo.Runtime {
    public sealed class NeoInt : NeoNumber {
        private static readonly int CACHE_SIZE = 256;
        private static readonly int HALF_CACHE_SIZE = CACHE_SIZE / 2;
        private static readonly NeoInt[] cache = new NeoInt[CACHE_SIZE];

        static NeoInt() {
            for (var i = 0; i < cache.Length; i++) {
                cache[i] = new NeoInt(i - HALF_CACHE_SIZE);
            }
        }

        public static NeoInt ValueOf(int i) {
            if (i >= -HALF_CACHE_SIZE && i <= (HALF_CACHE_SIZE - 1)) {
                return cache[i + HALF_CACHE_SIZE];
            }
            return new NeoInt(i);
        }

        private NeoInt(int value) {
            Value = value;
        }

        public int Value { get; }

        public override string Type => "int";

        public override int AsInt => Value;

        public override double AsDouble => Value;

        public override int Compare(NeoValue other) {
            if (other.IsInt) {
                return other.CheckInt().Value.CompareTo(Value);
            } else if (other.IsFloat) {
                return other.CheckFloat().Value.CompareTo(Value);
            }
            throw new NeoError($"Attempt to compare int to {other.Type}");
        }

        public override bool Equals(NeoValue other, bool deep) {
            if (other.IsInt) {
                return other.CheckInt().Value == Value;
            } else if (other.IsFloat) {
                return other.CheckFloat().Value == Value;
            } else {
                return false;
            }
        }

        public override string ToString() {
            return Value.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is NeoFloat f) {
                return f.Value == Value;
            } else if (obj is NeoInt i) {
                return i.Value == Value;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }
}
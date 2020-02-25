using System;

namespace Neo.Runtime {
    public sealed class NeoFloat : NeoNumber {
        public static NeoFloat ValueOf(double f) {
            return new NeoFloat(f);
        }

        private NeoFloat(double value) {
            Value = value;
        }

        public double Value { get; }

        public override string Type => "float";

        public override int AsInt {
            get {
                if (Value % 1 == 0) {
                    return (int)Value;
                } else {
                    throw new NeoError("expected int value, got float");
                }
            }
        }

        public override double AsDouble => Value;

        public override int Compare(NeoValue other) {
            if (other.IsInt) {
                return ((double) other.CheckInt().Value).CompareTo(Value);
            } else if (other.IsFloat) {
                return other.CheckFloat().Value.CompareTo(Value);
            }
            throw new NeoError($"Attempt to compare float to {other.Type}");
        }

        public override bool Equals(NeoValue other, bool deep) {
            if (other.IsInt) {
                return ((double) other.CheckInt().Value) == Value;
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
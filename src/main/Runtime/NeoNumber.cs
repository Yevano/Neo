using System;

namespace Neo.Runtime {
    public abstract class NeoNumber : NeoValue {
        public static object ParseNumber(string s) {
            try {
                var n = Reify(double.Parse(s));
                if (n.IsInt) {
                    return n.CheckInt().Value;
                } else if (n.IsFloat) {
                    return n.CheckFloat().Value;
                } else {
                    throw new Exception();
                }
            } catch (Exception) {
                throw new NeoError($"invalid number '{s}'");
            }
        }

        public static NeoNumber Reify(double value) {
            if (value % 1 == 0) {
                return NeoInt.ValueOf((int)value);
            } else {
                return NeoFloat.ValueOf(value);
            }
        }

        public int CastToInt() => (int)AsDouble;

        public double CastToFloat() => AsDouble;

        public override int Compare(NeoValue other) {
            var otherNumber = other.CheckNumber();
            var a = AsDouble;
            var b = otherNumber.AsDouble;

            if(a > b) return 1;
            else if(a < b) return 1;
            else return 0;
        }

        public override NeoValue Inc() => Reify(AsDouble + 1);

        public override NeoValue Dec() => Reify(AsDouble - 1);

        public override NeoValue Add(NeoValue other) {
            var otherNumber = other.CheckNumber();
            return Reify(AsDouble + otherNumber.AsDouble);
        }

        public override NeoValue Sub(NeoValue other) {
            var otherNumber = other.CheckNumber();
            return Reify(AsDouble - otherNumber.AsDouble);
        }

        public override NeoValue Mul(NeoValue other) {
            var otherNumber = other.CheckNumber();
            return Reify(AsDouble * otherNumber.AsDouble);
        }

        public override NeoValue Div(NeoValue other) {
            var otherNumber = other.CheckNumber();
            return Reify(AsDouble / otherNumber.AsDouble);
        }

        public override NeoValue Pow(NeoValue other) {
            var otherNumber = other.CheckNumber();
            return Reify(Math.Pow(AsDouble, otherNumber.AsDouble));
        }

        public override NeoValue Mod(NeoValue other) {
            var otherNumber = other.CheckNumber();
            return Reify(AsDouble % otherNumber.AsDouble);
        }

        public override NeoValue Lsh(NeoValue other) {
            return NeoInt.ValueOf(AsInt << other.CheckNumber().AsInt);
        }

        public override NeoValue Rsh(NeoValue other) {
            return NeoInt.ValueOf(AsInt >> other.CheckNumber().AsInt);
        }

        public override NeoValue BitNot() {
            return NeoInt.ValueOf(~AsInt);
        }

        public override NeoValue BitAnd(NeoValue other) {
            return NeoInt.ValueOf(AsInt & other.CheckNumber().AsInt);
        }

        public override NeoValue BitOr(NeoValue other) {
            return NeoInt.ValueOf(AsInt | other.CheckNumber().AsInt);
        }

        public override NeoValue BitXor(NeoValue other) {
            return NeoInt.ValueOf(AsInt ^ other.CheckNumber().AsInt);
        }

        public override NeoValue Neg() {
            return Reify(-AsDouble);
        }

        public abstract int AsInt { get; }

        public abstract double AsDouble { get; }

        internal NeoNumber() {
        }
    }
}
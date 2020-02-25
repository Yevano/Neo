namespace Neo.Runtime {
    public sealed class NeoString : NeoValue {
        public static NeoString ValueOf(string value) {
            return new NeoString(value);
        }

        private NeoString(string value) {
            Value = value;
        }

        public string ToUpper() => Value.ToUpper();

        public string ToLower() => Value.ToLower();

        public override NeoValue Length() => NeoInt.ValueOf(Value.Length);

        public override NeoValue Get(NeoValue key) {
            var index = key.CheckInt().Value;
            RangeCheck(index);
            return new NeoString(Value[index].ToString());
        }

        public override NeoValue Slice(NeoValue start, NeoValue end) {
            var s = start.CheckInt().Value;
            var e = end.CheckInt().Value;

            if (s < 0 || s >= Value.Length) throw new NeoError($"index out of bounds: {e}");
            if (e < 0 || e >= Value.Length) throw new NeoError($"index out of bounds: {e}");

            var d = s > e ? -1 : 1;

            var r = "";
            var i = s;
            while (i != e) {
                r += Value[i];
                i += d;
            }
            r += Value[i];

            return new NeoString(r);
        }

        private void RangeCheck(int index) {
            if (index < 0 || index >= Value.Length) {
                throw new NeoError($"index out of bounds: {index}; must be between 0 and {Value.Length - 1}");
            }
        }

        public string Value { get; }

        public override string Type => "string";

        public override string ToString() => Value;

        public override int Compare(NeoValue value) {
            if (value.IsString) {
                var str = value.CheckString();
                return Value.CompareTo(str.Value);
            } else {
                return -1;
            }
        }

        public override bool Equals(NeoValue value, bool deep) {
            if (value.IsString) {
                var str = value.CheckString();
                return Value.Equals(str.Value);
            } else {
                return false;
            }
        }

        public override bool Equals(object obj) {
            if (obj is NeoString s) {
                return s.Value == Value;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }
}
using System.Collections.Generic;
using System.Text;

namespace Neo.Runtime {
    public sealed class NeoArray : NeoValue {
        private readonly List<NeoValue> data;

        public NeoArray() {
            data = new List<NeoValue>();
        }

        public override void Insert(NeoValue value) => data.Add(value);

        public override void Remove(NeoValue key) => data.RemoveAt(key.CheckInt().Value);

        public override NeoValue Length() => NeoInt.ValueOf(data.Count);

        public override NeoValue Get(NeoValue key) => this[key.CheckInt().Value];

        public override void Set(NeoValue key, NeoValue value) => this[key.CheckInt().Value] = value;

        public override NeoValue Frozen() => new FrozenNeoArrayWrapper(this);

        public override NeoValue Slice(NeoValue start, NeoValue end) {
            var s = start.CheckInt().Value;
            var e = end.CheckInt().Value;
            var d = s > e ? -1 : 1;

            var a = new NeoArray();
            var i = s;
            while (i != e) {
                a.Insert(this[i]);
                i += d;
            }
            a.Insert(this[i]);

            return a;
        }

        private void RangeCheck(int index) {
            if (data.Count == 0) {
                throw new NeoError($"attempt to index ({index}) empty array");
            }

            if (index < 0 || index >= data.Count) {
                throw new NeoError($"index out of bounds: {index}; must be between 0 and {data.Count - 1}");
            }
        }

        public int Count => data.Count;

        public NeoValue this[int index] {
            get {
                RangeCheck(index);
                return data[index];
            }
            set {
                RangeCheck(index);
                data[index] = value;
            }
        }

        public override string Type => "array";

        public override bool Equals(NeoValue value, bool deep) {
            if (!value.IsArray) {
                return false;
            }

            var other = value.CheckArray();
            if (other.data.Count != data.Count) {
                return false;
            }

            if (deep) {
                for (var i = 0; i < data.Count; i++) {
                    if (!other[i].Equals(this[i], true)) {
                        return false;
                    }
                }
                return true;
            } else {
                return value == this;
            }
        }

        private string Stringify() {
            return Stringify(new HashSet<NeoValue>());
        }

        internal string Stringify(HashSet<NeoValue> seen) {
            if (data.Count == 0) {
                return "[]";
            }

            var builder = new StringBuilder();

            builder.Append("[ ");

            for (var i = 0; i < data.Count; i++) {
                builder.Append(NeoValue.StringifyElement(seen, data[i]));

                if (i + 1 < data.Count) {
                    builder.Append(", ");
                }
            }

            builder.Append(" ]");

            return builder.ToString();
        }

        public override string ToString() => Stringify();            
    }

    internal sealed class FrozenNeoArrayWrapper : NeoValue {
        private readonly NeoArray arr;

        public FrozenNeoArrayWrapper(NeoArray arr) {
            this.arr = arr;
        }

        public override void Insert(NeoValue value) => throw new NeoError("attempt to modify frozen array");

        public override void Remove(NeoValue key) => throw new NeoError("attempt to modify frozen array");

        public override NeoValue Length() => arr.Length();

        public override NeoValue Get(NeoValue key) => this[key.CheckInt().Value];

        public override void Set(NeoValue key, NeoValue value) => this[key.CheckInt().Value] = value;

        public override NeoValue Frozen() => this;

        public override NeoValue Slice(NeoValue start, NeoValue end) => arr.Slice(start, end);

        public int Count => arr.Count;

        public NeoValue this[int index] {
            get {
                return arr[index];
            }
            set {
                throw new NeoError("attempt to modify frozen array");
            }
        }

        public override string Type => "array";

        public override bool Equals(NeoValue value, bool deep) => arr.Equals(value, deep);

        public override string ToString() => arr.ToString();
    }
}
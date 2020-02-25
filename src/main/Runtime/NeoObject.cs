using System.Collections.Generic;
using System.Text;
using Neo.Utils;

namespace Neo.Runtime {
    public sealed class NeoObject : NeoValue {
        private readonly Dictionary<NeoValue, NeoValue> data;
        private readonly List<NeoValue> keys;

        public NeoObject() {
            data = new Dictionary<NeoValue, NeoValue>();
            keys = new List<NeoValue>();
        }

        private string Stringify() {
            return Stringify(new HashSet<NeoValue>());
        }

        internal string Stringify(HashSet<NeoValue> seen) {
            if (data.Count == 0) {
                return "{}";
            }

            var builder = new StringBuilder();

            builder.Append("{ ");

            var i = 1;
            foreach (var mapping in data) {
                builder.Append(NeoValue.StringifyElement(seen, mapping.Key));
                builder.Append(" = ");
                builder.Append(NeoValue.StringifyElement(seen, mapping.Value));

                if (i < data.Count) {
                    builder.Append(", ");
                }
                i++;
            }

            builder.Append(" }");

            return builder.ToString();
        }

        private NeoValue GetMetaMethod(string name, params NeoValue[] others) {
            var key = NeoString.ValueOf(name);

            if(MetaObject != null && MetaObject.data.ContainsKey(key)) return MetaObject.data[key];

            foreach(var v in others) {
                if(!v.IsObject) continue;
                var obj = (NeoObject) v;
                if(obj.MetaObject != null && obj.MetaObject.data.ContainsKey(key)) return obj.MetaObject.data[key];
            }

            return null;
        }

        public NeoObject MetaObject { get; set; }

        public NeoValue RawGet(NeoValue key) {
        	if(data.ContainsKey(key)) return data[key];
        	else return NeoNil.NIL;
        }

        public void RawSet(NeoValue key, NeoValue value) {
        	data[key] = value;
        }

        public NeoValue Index(int index) {
            if (data.Count == 0) {
                throw new NeoError($"attempt to index ({index}) empty object");
            }

            return keys[index];
        }

        public override string Type => "object";

        public override string ToNeoString() {
            var mm = GetMetaMethod("__tostring");
            if(mm != null) {
                return mm.Call(new [] { this }).CheckString().Value;
            } else {
                return Stringify();
            }
        }       

        public override bool Equals(NeoValue value, bool deep) {
            var mm = GetMetaMethod("__equals", value);
            if(mm != null) {
                var r = mm.Call(new [] { this, value, NeoBool.ValueOf(deep) });
                return r.CheckBool().Value;
            } else {
                if (!value.IsObject) {
                    return false;
                }

                var other = value.CheckObject();
                if (other.data.Count != data.Count) {
                    return false;
                }

                if (deep) {
                    foreach (var mapping in data) {
                        if (!mapping.Value.Equals(other.Get(mapping.Key), true)) {
                            return false;
                        }
                    }
                    return true;
                } else {
                    return this == other;
                }
            }
        }

        public override int Compare(NeoValue other) {
            var mm = GetMetaMethod("__compare", other);
            if(mm != null) {
                var r = mm.Call(new [] { this, other });
                return r.CheckInt().Value;
            } else {
                if (!other.IsObject) {
                    return base.Compare(other);
                }

                return base.Compare(other);
            }
        }

        public override NeoValue Get(NeoValue key) {
            var mm = GetMetaMethod("__get");
            if(mm != null) {
                return mm.Call(new [] { this, key });
            } else {
                if(data.ContainsKey(key)) return data[key];
                else return NeoNil.NIL;
            }
        }

        public override void Set(NeoValue key, NeoValue value) {
            var mm = GetMetaMethod("__set");
            if(mm != null) {
                mm.Call(new [] { this, key, value });
            } else {
                if(!data.ContainsKey(key)) keys.Add(key);
                data[key] = value;         
            }
        }

        public override NeoValue Call(NeoValue[] arguments) {
            var mm = GetMetaMethod("__call");
            if(mm != null) {
                return mm.Call(Arrays.Concat(new [] { this }, arguments));
            } else {
                throw new NeoError($"attempt to call object");
            }
        }

        public override NeoValue Inc() {
            var mm = GetMetaMethod("__inc");
            if(mm != null) {
                return mm.Call(new [] { this });
            } else {
                throw new NeoError($"attempt to increment {Type}");
            }
        }

        public override NeoValue Dec() {
            var mm = GetMetaMethod("__dec");
            if(mm != null) {
                return mm.Call(new [] { this });
            } else {
                throw new NeoError($"attempt to increment {Type}");
            }
        }

        public override NeoValue Add(NeoValue other) {
            var mm = GetMetaMethod("__add", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to add {Type} and {other.Type}");
            }
        }

        public override NeoValue Sub(NeoValue other) {
            var mm = GetMetaMethod("__sub", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to subtract {Type} and {other.Type}");
            }
        }

        public override NeoValue Mul(NeoValue other) {
            var mm = GetMetaMethod("__mul", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to multiply {Type} and {other.Type}");
            }
        }

        public override NeoValue Div(NeoValue other) {
            var mm = GetMetaMethod("__div", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to divide {Type} and {other.Type}");
            }
        }

        public override NeoValue Pow(NeoValue other) {
            var mm = GetMetaMethod("__pow", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to exponentiate {Type} and {other.Type}");
            }
        }

        public override NeoValue Mod(NeoValue other) {
            var mm = GetMetaMethod("__mod", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to modulus {Type} and {other.Type}");
            }
        }

        public override NeoValue Lsh(NeoValue other) {
            var mm = GetMetaMethod("__lsh", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to left-shift {Type} and {other.Type}");
            }
        }

        public override NeoValue Rsh(NeoValue other) {
            var mm = GetMetaMethod("__rsh", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to right-shift {Type} and {other.Type}");
            }
        }

        public override NeoValue BitNot() {
            var mm = GetMetaMethod("__bitnot");
            if(mm != null) {
                return mm.Call(new [] { this });
            } else {
                throw new NeoError($"attempt to bit-not {Type}");
            }
        }

        public override NeoValue BitAnd(NeoValue other) {
            var mm = GetMetaMethod("__bitand", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to bit-and {Type} and {other.Type}");
            }
        }

        public override NeoValue BitOr(NeoValue other) {
            var mm = GetMetaMethod("__bitor", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to bit-or {Type} and {other.Type}");
            }
        }

        public override NeoValue BitXor(NeoValue other) {
            var mm = GetMetaMethod("__bitxor", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to bit-xor {Type} and {other.Type}");
            }
        }

        public override NeoValue Not() {
            var mm = GetMetaMethod("__not");
            if(mm != null) {
                return mm.Call(new [] { this });
            } else {
                throw new NeoError($"attempt to not {Type}");
            }
        }

        public override NeoValue Neg() {
            var mm = GetMetaMethod("__neg");
            if(mm != null) {
                return mm.Call(new [] { this });
            } else {
                throw new NeoError($"attempt to negate {Type}");
            }
        }        

        public override NeoValue Concat(NeoValue other) {
            var mm = GetMetaMethod("__concat", other);
            if(mm != null) {
                return mm.Call(new [] { this, other });
            } else {
                throw new NeoError($"attempt to concat {Type} and {other.Type}");
            }
        }

        public override NeoValue Length() {
            var mm = GetMetaMethod("__length");
            if(mm != null) {
                return mm.Call(new [] { this });
            } else {
                return NeoInt.ValueOf(data.Count);
            }
        }

        public override NeoValue Slice(NeoValue start, NeoValue end) {
            var mm = GetMetaMethod("__slice");
            if(mm != null) {
                return mm.Call(new [] { this, start, end });
            } else {
                throw new NeoError($"attenpt to slice {Type}");
            }
        }

        public override NeoValue Frozen() {
            var mm = GetMetaMethod("__frozen");
            if(mm != null) {
                return mm.Call(new [] { this });
            } else {
                return new FrozenNeoObjectWrapper(this);
            }
        }

        public override void Remove(NeoValue key) => data.Remove(key);
    }

    internal sealed class FrozenNeoObjectWrapper : NeoValue {
        private readonly NeoObject obj;

        public FrozenNeoObjectWrapper(NeoObject obj) {
            this.obj = obj;
        }

        public NeoObject MetaObject {
            get {
                return obj.MetaObject;
            }
            set {
                throw new NeoError("attempt to modify frozen object");
            }
        }

        public override string ToNeoString() => obj.ToNeoString();

        public override bool Equals(NeoValue value, bool deep) => obj.Equals(value, deep);

        public override int Compare(NeoValue other) => obj.Compare(other);

        public override NeoValue Get(NeoValue key) => obj.Get(key);

        public override void Set(NeoValue key, NeoValue value) => obj.Set(key, value);
        
        public override NeoValue Call(NeoValue[] arguments) => obj.Call(arguments);

        public override NeoValue Inc() => obj.Inc();

        public override NeoValue Dec() => obj.Dec();

        public override NeoValue Add(NeoValue other) => obj.Add(other);

        public override NeoValue Sub(NeoValue other) => obj.Sub(other);

        public override NeoValue Mul(NeoValue other) => obj.Mul(other);

        public override NeoValue Div(NeoValue other) => obj.Div(other);

        public override NeoValue Pow(NeoValue other) => obj.Pow(other);

        public override NeoValue Mod(NeoValue other) => obj.Mod(other);

        public override NeoValue Lsh(NeoValue other) => obj.Lsh(other);

        public override NeoValue Rsh(NeoValue other) => obj.Rsh(other);

        public override NeoValue BitNot() => obj.BitNot();

        public override NeoValue BitAnd(NeoValue other) => obj.BitAnd(other);

        public override NeoValue BitOr(NeoValue other) => obj.BitOr(other);

        public override NeoValue BitXor(NeoValue other) => obj.BitXor(other);

        public override NeoValue Not() => obj.Not();

        public override NeoValue Neg() => obj.Neg();

        public override NeoValue Concat(NeoValue other) => obj.Concat(other);

        public override NeoValue Length() => obj.Length();

        public override NeoValue Slice(NeoValue start, NeoValue end) => obj.Slice(start, end);

        public override NeoValue Frozen() => this;

        public NeoValue Index(int index) => obj.Index(index);

        public override void Remove(NeoValue key) => throw new NeoError("attempt to modify frozen object");

        public override string Type => "object";
    }
}
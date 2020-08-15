using System.Collections.Generic;
using System;
using Neo.Utils;

namespace Neo.Runtime {
    public abstract class NeoValue {
        internal static string StringifyElement(HashSet<NeoValue> seen, NeoValue value) {
            if(value is NeoObject o) {
                if(seen.Contains(value)) {
                    return "<already-seen>";
                } else {
                    seen.Add(value);
                    return o.Stringify(seen);
                }
            } else if(value is NeoArray a) {
                if(seen.Contains(value)) {
                    return "[already-seen]";
                } else {
                    seen.Add(value);
                    return a.Stringify(seen);
                }
            } else {
                return value.ToNeoString();
            }
        }

        private static string TypeName<T>() where T : NeoValue {
            string name = null;

            TypeSwitch.Do<T>(
                TypeSwitch.Case<NeoProcedure>(() => name = "procedure"),
                TypeSwitch.Case<NeoObject>(()    => name = "object"),
                TypeSwitch.Case<NeoArray>(()     => name = "array"),
                TypeSwitch.Case<NeoFloat>(()     => name = "float"),
                TypeSwitch.Case<NeoNumber>(()    => name = "number"),
                TypeSwitch.Case<NeoInt>(()       => name = "int"),
                TypeSwitch.Case<NeoBool>(()      => name = "bool"),
                TypeSwitch.Default(()            => name = typeof(T).ToString())
            );

            return name;
        }

        internal NeoValue() {
        }

        public bool Is<T>() {
            return this is T;
        }

        public T Check<T>() where T : NeoValue {
            if (this is T) {
                return (T)this;
            }
            throw new NeoError($"attempt to cast {Type} to {TypeName<T>()}");
        }

        public bool IsArray => Is<NeoArray>() || Is<FrozenNeoArrayWrapper>();

        public bool IsBool => Is<NeoBool>();

        public bool IsFloat => Is<NeoFloat>();

        public bool IsInt => Is<NeoInt>();

        public bool IsNil => Is<NeoNil>();

        public bool IsNumber => Is<NeoNumber>();

        public bool IsObject => Is<NeoObject>() || Is<FrozenNeoObjectWrapper>();

        public bool IsProcedure => Is<NeoProcedure>();

        public bool IsString => Is<NeoString>();

        public NeoArray CheckArray() => Check<NeoArray>();

        public NeoBool CheckBool() => Check<NeoBool>();

        public NeoFloat CheckFloat() => Check<NeoFloat>();

        public NeoInt CheckInt() => Check<NeoInt>();

        public NeoNil CheckNil() => Check<NeoNil>();

        public NeoNumber CheckNumber() => Check<NeoNumber>();

        public NeoObject CheckObject() => Check<NeoObject>();

        public NeoProcedure CheckProcedure() => Check<NeoProcedure>();

        public NeoString CheckString() => Check<NeoString>();

        public abstract string Type { get; }

        public NeoValue Eq(NeoValue other) => Equals(other, false) ? NeoBool.TRUE : NeoBool.FALSE;

        public NeoValue Ne(NeoValue other) => Equals(other, false) ? NeoBool.FALSE : NeoBool.TRUE;

        public NeoValue DeepEq(NeoValue other) => Equals(other, true) ? NeoBool.TRUE : NeoBool.FALSE;

        public NeoValue DeepNe(NeoValue other) => Equals(other, true) ? NeoBool.FALSE : NeoBool.TRUE;

        public NeoValue Lt(NeoValue other) => NeoBool.ValueOf(Compare(other) > 0);

        public NeoValue Gt(NeoValue other) => NeoBool.ValueOf(Compare(other) < 0);

        public NeoValue Lte(NeoValue other) => NeoBool.ValueOf(Compare(other) >= 0);

        public NeoValue Gte(NeoValue other) => NeoBool.ValueOf(Compare(other) <= 0);

        public override string ToString() => Type;

        public virtual string ToNeoString() => ToString();

        public virtual bool Equals(NeoValue value, bool deep) => this == value;

        public virtual int Compare(NeoValue other) => throw new NeoError($"attempt to compare {Type} to {other.Type}");

        public virtual NeoValue Get(NeoValue key) => throw new NeoError($"attempt to index {Type}");

        public virtual void Set(NeoValue key, NeoValue value) => throw new NeoError($"attempt to index {Type}");

        public virtual NeoValue Call(NeoValue[] arguments) => throw new NeoError($"attempt to call {Type}");

        public virtual NeoValue Inc() => throw new NeoError($"attempt to increment {Type}");

        public virtual NeoValue Dec() => throw new NeoError($"attempt to decrement {Type}");

        public virtual NeoValue Add(NeoValue other) => throw new NeoError($"attempt to add {Type} and {other.Type}");

        public virtual NeoValue Sub(NeoValue other) => throw new NeoError($"attempt to subtract {Type} and {other.Type}");

        public virtual NeoValue Mul(NeoValue other) => throw new NeoError($"attempt to multiply {Type} and {other.Type}");

        public virtual NeoValue Div(NeoValue other) => throw new NeoError($"attempt to divide {Type} and {other.Type}");

        public virtual NeoValue Pow(NeoValue other) => throw new NeoError($"attempt to exponentiate {Type} and {other.Type}");

        public virtual NeoValue Mod(NeoValue other) => throw new NeoError($"attempt to modulus {Type} and {other.Type}");

        public virtual NeoValue Lsh(NeoValue other) => throw new NeoError($"attempt to left-shift {Type} and {other.Type}");

        public virtual NeoValue Rsh(NeoValue other) => throw new NeoError($"attempt to right-shift {Type} and {other.Type}");

        public virtual NeoValue BitNot() => throw new NeoError($"attempt to bit-not {Type}");

        public virtual NeoValue BitAnd(NeoValue other) => throw new NeoError($"attempt to bit-and {Type} and {other.Type}");

        public virtual NeoValue BitOr(NeoValue other) => throw new NeoError($"attempt to bit-or {Type} and {other.Type}");

        public virtual NeoValue BitXor(NeoValue other) => throw new NeoError($"attempt to bit-xor {Type} and {other.Type}");

        public virtual NeoValue Not() => throw new NeoError($"attempt to not {Type}");

        public virtual NeoValue Neg() => throw new NeoError($"attempt to negate {Type}");

        public virtual NeoValue Concat(NeoValue other) => NeoString.ValueOf($"{ToNeoString()}{other.ToNeoString()}");

        public virtual NeoValue Length() => throw new NeoError($"attempt to get length of {Type}");

        public virtual NeoValue Slice(NeoValue start, NeoValue end) => throw new NeoError($"attenpt to slice {Type}");

        public virtual NeoValue Frozen() => this;

        public virtual void Insert(NeoValue value) => throw new NeoError($"attempt to insert {Type}");

        public virtual void Remove(NeoValue key) => throw new NeoError($"attempt to remove {Type}");
    }
}
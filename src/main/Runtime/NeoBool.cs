namespace Neo.Runtime {
    public sealed class NeoBool : NeoValue {
        public static readonly NeoBool TRUE = new NeoBool(true);
        public static readonly NeoBool FALSE = new NeoBool(false);

        public static NeoBool ValueOf(bool b) {
            return b ? TRUE : FALSE;
        }

        private NeoBool(bool value) {
            Value = value;
        }

        public bool Value { get; }

        public override string Type => "bool";

        public override bool Equals(NeoValue other, bool deep) {
            if (other.IsBool) {
                return other.CheckBool().Value == Value;
            } else {
                return false;
            }
        }

        public override NeoValue Not() {
            return Value ? FALSE : TRUE;
        }

        public override string ToString() {
            return Value.ToString().ToLower();
        }
    }
}
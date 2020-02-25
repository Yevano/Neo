namespace Neo.Runtime {
    public sealed class NeoNil : NeoValue {
        public static readonly NeoNil NIL = new NeoNil();

        private NeoNil() {
        }

        public override string Type => "nil";

        public override string ToString() {
            return "nil";
        }

        public override bool Equals(NeoValue other, bool deep) => other == this;
    }
}
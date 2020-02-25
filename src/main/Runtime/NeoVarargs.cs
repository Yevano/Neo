namespace Neo.Runtime {
    public sealed class NeoSpreadValue : NeoValue {
        public NeoSpreadValue(NeoArray array) {
            Array = array;
        }

        public NeoArray Array { get; }

        public override string Type => "varargs";
    }
}

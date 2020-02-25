using Neo.Bytecode;
using Neo.Runtime.Internal;

namespace Neo.Runtime {
    public sealed class NeoChunk : NeoValue {
        internal NeoChunk(Chunk chunk) {
            Chunk = chunk;
            Scope = new Scope();
        }

        public override NeoValue Length() => NeoInt.ValueOf(Scope.LocalCount);

        public override NeoValue Get(NeoValue key) {
        	var name = key.CheckString().Value;
            if (Scope.IsExported(name)) {
                return Scope.Get(name);
            } else {
                return NeoNil.NIL;
            }
        }

        public override void Set(NeoValue key, NeoValue value) => throw new NeoError("attempt to modify chunk");

        public Chunk Chunk { get; }

        public Scope Scope { get; }

        public override string Type => "chunk";

        public override bool Equals(NeoValue value, bool deep) {
            return value == this;
        }
    }
}
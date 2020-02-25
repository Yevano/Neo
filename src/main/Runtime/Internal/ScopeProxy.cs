namespace Neo.Runtime.Internal {
    public sealed class ScopeProxy : NeoValue {
        public ScopeProxy(Scope scope) {
            Scope = scope;
        }

        public Scope Scope { get; }

        public override NeoValue Get(NeoValue key) {
        	var name = key.CheckString().Value;
            if (Scope.IsExported(name)) {
                return Scope.Get(name);
            } else {
                return NeoNil.NIL;
            }
        }

        public override void Set(NeoValue key, NeoValue value) => Scope.Set(key.CheckString().Value, value);

        public override string Type => "scope-proxy";
    }
}

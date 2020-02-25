namespace Neo.Runtime.Internal {
    public sealed class UpValue {
        private NeoValue value;

        public UpValue(Scope scope, string name) {
            Scope = scope;
            Name = name;
        }

        public UpValue(NeoValue value, string name) {
            this.value = value;
            Name = name;
        }

        public Scope Scope { get; }

        public string Name { get; }

        public NeoValue Get() {
            if (value == null) {
                return Scope.Get(Name);
            } else {
                return value;
            }
        }

        public void Set(NeoValue value) {
            if (this.value == null) {
                Scope.Set(Name, value);
            } else {
                this.value = value;
            }
        }

        public void Close() {
            value = Scope.Get(Name);
        }
    }
}
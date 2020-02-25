namespace Neo.Bytecode {
    public struct Parameter {
        public Parameter(string name, bool frozen) {
            Name = name;
            Frozen = frozen;
        }

        public string Name { get; }

        public bool Frozen { get; }
    }
}
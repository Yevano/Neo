using System;

namespace Neo.Bytecode {
	[Flags]
	public enum VariableFlags : byte {
		NONE = 0,
		EXPORTED = 1 << 0,
		FINAL = 1 << 1
	}

    public struct Variable {
        public Variable(string name, VariableFlags flags) {
            Name = name;
            Flags = flags;
        }

        public string Name { get; }

        public VariableFlags Flags { get; }
    }
}
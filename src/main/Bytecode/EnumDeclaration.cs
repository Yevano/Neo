using System.Collections.Generic;

namespace Neo.Bytecode {
    public sealed class EnumDeclaration {
        public EnumDeclaration(string name, List<string> elements, bool exported) {
            Name = name;
            Elements = elements;
            Exported = exported;
        }

        public string Name { get; }

        public List<string> Elements { get; }

        public bool Exported { get; }
    }
}
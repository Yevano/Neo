using System;
using System.Collections.Generic;
using Neo.Bytecode;

namespace Neo.Runtime.Internal {
    public sealed class Scope {
        private static readonly NeoValue SENTINEL = NeoProcedure.FromLambda(args => throw new Exception("SENTINEL should never be called"));

        private readonly Dictionary<string, NeoValue> bindings;
        private readonly HashSet<string> exports;
        private readonly HashSet<string> finals;
        private readonly Dictionary<string, Scope> importedNames;
        private readonly List<Scope> imports;
        private readonly ScopeProxy proxy;

        public Scope(Scope parent = null) {
            if (parent == null) {
                Parent = null;
                Level = 0;
            } else {
                Parent = parent;
                Level = Parent.Level + 1;
            }

            bindings = new Dictionary<string, NeoValue>();
            exports = new HashSet<string>();
            finals = new HashSet<string>();
            importedNames = new Dictionary<string, Scope>();
            imports = new List<Scope>();
            proxy = new ScopeProxy(this);
        }

        public ScopeProxy Proxy => proxy;

        public int LocalCount => bindings.Count;

        public Scope Parent { get; }

        public int Level { get; }

        public bool IsTopLevel => Parent == null;

        public void Export(string name) => exports.Add(name);

        public bool IsExported(string name) => exports.Contains(name);

        public IEnumerable<string> Exports {
            get {
                foreach (var export in exports) {
                    yield return export;
                }
            }
        }

        public void Import(Scope scope) {
            if (imports.Contains(scope)) return;

            imports.Add(scope);

            foreach (var export in scope.Exports) {
                importedNames[export] = scope;
            }
        }

        public void MarkFinal(string name) => finals.Add(name);

        public bool IsFinal(string name) => finals.Contains(name);

        public void Declare(string name, VariableFlags flags) {
            bindings[name] = SENTINEL;
            if (flags.HasFlag(VariableFlags.EXPORTED)) Export(name);
            if (flags.HasFlag(VariableFlags.FINAL)) MarkFinal(name);
        }

        public Scope FindDeclaringScope(string name) {
            if (bindings.ContainsKey(name)) {
                return this;
            }

            if (Parent != null) {
                var result = Parent.FindDeclaringScope(name);
                if (result != null) {
                    return result;
                }
            }

            if (importedNames.TryGetValue(name, out var scope)) {
                return scope;
            }

            return null;
        }

        public void Set(string name, NeoValue value) {
            var declaringScope = FindDeclaringScope(name);
            if (declaringScope == null) throw new NeoError($"attempt to access undeclared value '{name}'");
            declaringScope.RawSet(name, value);
        }

        public NeoValue Get(string name) {
            var declaringScope = FindDeclaringScope(name);
            if (declaringScope == null) throw new NeoError($"attempt to access undeclared value '{name}'");
            return declaringScope.RawGet(name);
        }

        private void RawSet(string name, NeoValue value) {
            if (IsFinal(name) && bindings[name] != SENTINEL) throw new NeoError($"attempt to modify final value '{name}'");
            bindings[name] = value;
        }

        private NeoValue RawGet(string name) {
            if (bindings.TryGetValue(name, out var value)) {
                if (value == SENTINEL) return NeoNil.NIL;
                return value;
            }

            throw new Exception();
        }
    }
}
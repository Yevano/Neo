using Neo.Bytecode;
using Neo.Runtime.Internal;

namespace Neo.Backend.JIT {
    public sealed class JITBackend : IBackend {
        public JITBackend(VM vm) {
            VM = vm;
        }

        public VM VM { get; }

        public IProcedureImplementation Compile(Scope scope, Chunk chunk, Procedure proc, UpValue[] upvalues) {
            var compiler = new ProcedureCompiler(VM, scope, chunk, proc, upvalues);
            return compiler.Compile();
        }
    }
}
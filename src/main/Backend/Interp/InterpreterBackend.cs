using Neo.Bytecode;
using Neo.Runtime.Internal;

namespace Neo.Backend.Interp {
    public sealed class InterpreterBackend : IBackend {
        internal InterpreterBackend(VM vm) {
            VM = vm;
        }

        public VM VM { get; }

        public IProcedureImplementation Compile(Scope scope, Chunk chunk, Procedure proc, UpValue[] upvalues) {
            return new InterpProcedure(VM, scope, chunk, proc, upvalues);
        }
    }
}
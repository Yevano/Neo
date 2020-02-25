using Neo.Backend.Interp;
using Neo.Runtime;

namespace Neo.Backend {
    internal sealed class NeoBackendProcedure : NeoProcedure {
        private static readonly int JIT_THRESHOLD = 25;

        private readonly VM vm;
        private bool jitted;
        private IProcedureImplementation proc;

        private int calls;

        public NeoBackendProcedure(VM vm, IProcedureImplementation proc) {
            this.vm = vm;
            this.proc = proc;
        }

        public IProcedureImplementation Implementation => proc;

        public override NeoValue Call(NeoValue[] arguments) {
            calls++;

            var result = proc.Call(arguments);

            if (!jitted && calls >= JIT_THRESHOLD) {
                calls = 0;
                JIT();
            }

            return result;
        }

        public override string Name() => proc.Name();

        public override string ChunkName() => proc.ChunkName();

        public void JIT() {
            if(!jitted && proc is InterpProcedure iproc) {
                proc = vm.JIT.Compile(iproc.ParentScope, iproc.Chunk, iproc.Procedure, iproc.UpValues);
                jitted = true;
            }
        }
    }
}
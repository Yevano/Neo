using Neo.Bytecode;

namespace Neo.Runtime.Internal {
    public struct Frame {
        public Frame(string chunkName, string procedureName, int callSiteLine) {
            ChunkName = chunkName;
            ProcedureName = procedureName;
            CallSiteLine = callSiteLine;
        }

        public string ChunkName { get; }

        public string ProcedureName { get; }
    	
    	public int CallSiteLine { get; }
    }
}
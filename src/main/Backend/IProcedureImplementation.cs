using Neo.Runtime;

namespace Neo.Backend {
    public interface IProcedureImplementation {
        NeoValue Call(NeoValue[] args);

        string Name();
    	
    	string ChunkName();
    }
}
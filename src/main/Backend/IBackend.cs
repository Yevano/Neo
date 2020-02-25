using Neo.Bytecode;
using Neo.Runtime.Internal;

namespace Neo.Backend {
    public interface IBackend {
        IProcedureImplementation Compile(Scope scope, Chunk chunk, Procedure procedure, UpValue[] upvalues);
    }
}
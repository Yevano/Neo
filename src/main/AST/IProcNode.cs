using Neo.AST.Declarations;
using Neo.AST.Statements;
using System.Collections.Generic;

namespace Neo.AST {
    public interface IProcNode {
        List<ParameterNode> Parameters { get; }

        List<StatementNode> Statements { get; }

        string Name { get; }
    }
}
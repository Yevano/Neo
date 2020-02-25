using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public abstract class StatementNode : Node {
        internal StatementNode(SourcePosition position) : base(position) {
        }
    }
}
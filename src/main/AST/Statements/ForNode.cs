using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public abstract class ForNode : StatementNode {
        internal ForNode(SourcePosition position) : base(position) {
        }
    }
}

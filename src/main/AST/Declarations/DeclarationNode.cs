using Neo.AST.Statements;
using Neo.Frontend.Lexer;

namespace Neo.AST.Declarations {
    public abstract class DeclarationNode : StatementNode {
        internal DeclarationNode(SourcePosition position) : base(position) {
        }
    }
}
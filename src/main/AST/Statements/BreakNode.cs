using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class BreakNode : StatementNode {
        public BreakNode(SourcePosition position) : base(position) {
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitBreak(this);
        }
    }
}
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class ContinueNode : StatementNode {
        public ContinueNode(SourcePosition position) : base(position) {
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitContinue(this);
        }
    }
}
using Neo.AST.Expressions;
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class IfNode : StatementNode {
        public IfNode(SourcePosition position, ExpressionNode condition, StatementNode @true, StatementNode @false) : base(position) {
            Condition = condition;
            True = @true;
            False = @false;
        }

        public ExpressionNode Condition;

        public StatementNode True { get; }

        public StatementNode False { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitIf(this);
        }
    }
}

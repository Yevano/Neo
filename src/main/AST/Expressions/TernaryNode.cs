using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions {
    public sealed class TernaryNode : ExpressionNode {
        public TernaryNode(SourcePosition position, ExpressionNode condition, ExpressionNode @true, ExpressionNode @false) : base(position) {
            Condition = condition;
            True = @true;
            False = @false;
        }

        public ExpressionNode Condition { get; }

        public ExpressionNode True { get; }

        public ExpressionNode False { get; }

        public override bool IsConstant() {
            return Condition.IsConstant() && True.IsConstant() && False.IsConstant();
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitTernary(this);
        }
    }
}
using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions {
    public sealed class ParenNode : ExpressionNode {
        public ParenNode(SourcePosition position, ExpressionNode value) : base(position) {
            Value = value;
        }

        public ExpressionNode Value { get; }

        public override bool IsConstant() {
        	return Value.IsConstant();
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitParen(this);
        }
    }
}
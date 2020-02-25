using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions {
    public sealed class UnaryNode : ExpressionNode {
        public UnaryNode(SourcePosition position, UnaryOP op, ExpressionNode value) : base(position) {
            OP = op;
            Value = value;
        }

        public UnaryOP OP { get; }

        public ExpressionNode Value { get; }

        public override bool IsConstant() {
            return Value.IsConstant();
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitUnary(this);
        }
    }

    public enum UnaryOP {
        BIT_NOT,
        NOT,
        LENGTH,
        NEG,
        UNPACK,
        FROZEN,

        PREFIX_INCREMENT,
        POSTFIX_INCREMENT,
        PREFIX_DECREMENT,
        POSTFIX_DECREMENT,
    }
}

using Neo.AST.Expressions;
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class ThrowNode : StatementNode {
        public ThrowNode(SourcePosition position, ExpressionNode value) : base(position) {
            Value = value;
        }

        public ExpressionNode Value { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitThrow(this);
        }
    }
}
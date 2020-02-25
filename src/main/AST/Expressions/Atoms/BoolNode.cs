using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions.Atoms {
    public sealed class BoolNode : ExpressionNode {
        public BoolNode(SourcePosition position, bool value) : base(position) {
            Value = value;
        }

        public bool Value { get; }

        public override bool IsConstant() {
            return true;
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitBool(this);
        }
    }
}
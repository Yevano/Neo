using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions.Atoms {
    public sealed class FloatNode : NumberNode {
        public FloatNode(SourcePosition position, double value) : base(position) {
            Value = value;
        }

        public double Value { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitFloat(this);
        }
    }
}
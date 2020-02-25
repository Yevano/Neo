using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions.Atoms {
    public sealed class IntNode : NumberNode {
        public IntNode(SourcePosition position, int value) : base(position) {
            Value = value;
        }

        public int Value { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitInt(this);
        }
    }
}

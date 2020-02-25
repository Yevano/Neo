using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions.Atoms {
    public sealed class StringNode : ExpressionNode {
        public StringNode(SourcePosition position, string value) : base(position) {
            Value = value;
        }

        public string Value { get; }

        public override bool IsConstant() {
         	return true;   
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitString(this);
        }
    }
}
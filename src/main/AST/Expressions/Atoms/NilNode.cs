using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions.Atoms {
    public sealed class NilNode : ExpressionNode {
        public NilNode(SourcePosition position) : base(position) {
        }

        public override bool IsConstant() {
        	return true;
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitNil(this);
        }
    }
}
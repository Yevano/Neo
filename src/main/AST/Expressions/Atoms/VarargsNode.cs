using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions.Atoms {
    public sealed class VarargsNode : ExpressionNode {
    	public VarargsNode(SourcePosition position) : base(position) {
    	}
    	
        public override bool IsConstant() {
        	return false;
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitVarargs(this);
        }
    }
}
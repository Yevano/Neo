using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions.Atoms {
    public abstract class NumberNode : ExpressionNode {
        internal NumberNode(SourcePosition position) : base(position) {
        }

        public override bool IsConstant() {
        	return true;
        }
    }
}
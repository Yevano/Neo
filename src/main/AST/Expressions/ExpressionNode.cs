using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions {
    public abstract class ExpressionNode : Node {
        internal ExpressionNode(SourcePosition position) : base(position) {
        }

        public abstract bool IsConstant();
    }
}
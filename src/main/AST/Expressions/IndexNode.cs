using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions {
    public sealed class IndexNode : ExpressionNode {
        public IndexNode(SourcePosition position, ExpressionNode value, ExpressionNode key) : base(position) {
            Value = value;
            Key = key;
        }
            
        public ExpressionNode Value { get; }

        public ExpressionNode Key { get; }

        public override bool IsConstant() {
            return Value.IsConstant() && Key.IsConstant();  
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitIndex(this);
        }
    }
}
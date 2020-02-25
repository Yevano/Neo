using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions {
    public sealed class SliceNode : ExpressionNode {
        public SliceNode(SourcePosition position, ExpressionNode array, ExpressionNode start, ExpressionNode end) : base(position) {
            Array = array;
            Start = start;
            End = end;
        }

        public ExpressionNode Array { get; }

        public ExpressionNode Start { get; }

        public ExpressionNode End { get; }

        public override bool IsConstant() {
            return Array.IsConstant() && Start.IsConstant() && End.IsConstant();
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitSlice(this);
        }
    }
}
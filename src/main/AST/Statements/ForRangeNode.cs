using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class ForRangeNode : ForNode {
        public ForRangeNode(SourcePosition position, IdentNode iterator, ExpressionNode start, ExpressionNode end, ExpressionNode by, StatementNode code) : base(position) {
            Iterator = iterator;
            Start = start;
            End = end;
            Code = code;
            By = by;
        }

        public IdentNode Iterator { get; }

        public ExpressionNode Start { get; }

        public ExpressionNode End { get; }

        public ExpressionNode By { get; }

        public StatementNode Code { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitForRange(this);
        }
    }
}
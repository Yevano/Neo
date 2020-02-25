using Neo.AST.Expressions;
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class WhileNode : StatementNode {
        public WhileNode(SourcePosition position, ExpressionNode condition, StatementNode code) : base(position) {
            Condition = condition;
            Code = code;
        }

        public ExpressionNode Condition { get; }

        public StatementNode Code { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitWhile(this);
        }
    }
}
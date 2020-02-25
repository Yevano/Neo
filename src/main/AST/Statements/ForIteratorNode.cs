using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class ForIteratorNode : ForNode {
        public ForIteratorNode(SourcePosition position, IdentNode iterator, ExpressionNode from, StatementNode code) : base(position) {
            Iterator = iterator;
            From = from;
            Code = code;
        }

        public IdentNode Iterator { get; }

        public ExpressionNode From { get; }

        public StatementNode Code { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitForIterator(this);
        }
    }
}
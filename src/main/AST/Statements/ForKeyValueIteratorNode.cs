using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class ForKeyValueIteratorNode : ForNode {
        public ForKeyValueIteratorNode(SourcePosition position, IdentNode key, IdentNode value, ExpressionNode from, StatementNode code) : base(position) {
            Key = key;
            Value = value;
            From = from;
            Code = code;
        }

        public IdentNode Key { get; }

        public IdentNode Value { get; }

        public ExpressionNode From { get; }

        public StatementNode Code { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitForKeyValueIterator(this);
        }
    }
}

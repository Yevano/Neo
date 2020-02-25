using Neo.Frontend.Lexer;
using Neo.AST.Expressions;

namespace Neo.AST.Statements {
    public sealed class AssignNode : StatementNode {
        public AssignNode(SourcePosition position, AssignOP op, ExpressionNode left, ExpressionNode right) : base(position) {
            OP = op;
            Left = left;
            Right = right;
        }

        public AssignOP OP { get; }

        public ExpressionNode Left { get; }

        public ExpressionNode Right { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitAssign(this);
        }
    }

    public enum AssignOP {
        NORMAL,
    
        ADD,
        SUB,
        MUL,
        DIV,
        POW,
        MOD,

        LSH,
        RSH,

        BIT_NOT,
        BIT_AND,
        BIT_OR,
        BIT_XOR,

        AND,
        OR,

        CONCAT
    }
}
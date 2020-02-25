using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions {
    public sealed class BinaryNode : ExpressionNode {
        public BinaryNode(SourcePosition position, BinaryOP op, ExpressionNode left, ExpressionNode right) : base(position) {
            OP = op;
            Left = left;
            Right = right;
        }

        public BinaryOP OP { get; }

        public ExpressionNode Left { get; }

        public ExpressionNode Right { get; }

        public override bool IsConstant() {
            return Left.IsConstant() && Right.IsConstant();   
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitBinary(this);
        }
    }

    public enum BinaryOP {
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

        CONCAT,

        EQ,
        NE,
        DEEP_EQ,
        DEEP_NE,
        LT,
        GT,
        LTE,
        GTE
    }
}
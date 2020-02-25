using Neo.Frontend.Lexer;
using System.Collections.Generic;

namespace Neo.AST.Expressions {
    public sealed class CallNode : ExpressionNode {
        public CallNode(SourcePosition position, ExpressionNode proc) : base(position) {
            Proc = proc;
            Arguments = new List<ExpressionNode>();
        }

        public ExpressionNode Proc { get; }
     
        public List<ExpressionNode> Arguments { get; }

        public override bool IsConstant() {
            return false;   
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitCall(this);
        }
    }
}
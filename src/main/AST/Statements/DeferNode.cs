using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class DeferNode : StatementNode {
        private static int currentID;

        public DeferNode(SourcePosition position, StatementNode code) : base(position) {
            Code = code;
            ID = currentID++;
        }

        public int ID { get; }

        public StatementNode Code { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitDefer(this);
        }
    }
}
using Neo.AST.Expressions.Atoms;
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class TryCatchNode : StatementNode {
        private static int currentID;

        public TryCatchNode(SourcePosition position, StatementNode @try, StatementNode @catch, IdentNode error) : base(position) {
            Try = @try;
            Catch = @catch;
            Error = error;
            ID = currentID++;
        }

        public int ID { get; }

        public StatementNode Try { get; }

        public StatementNode Catch { get; }

        public IdentNode Error { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitTryCatch(this);
        }
    }
}
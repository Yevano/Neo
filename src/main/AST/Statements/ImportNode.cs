using Neo.AST.Expressions.Atoms;
using Neo.Frontend.Lexer;

namespace Neo.AST.Statements {
    public sealed class ImportNode : StatementNode {
        public ImportNode(SourcePosition position, StringNode path) : base(position) {
            Path = path;
        }

        public StringNode Path { get; }

        public IdentNode Alias { get; set; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitImport(this);
        }
    }
}
using Neo.AST.Expressions.Atoms;
using Neo.Frontend.Lexer;

namespace Neo.AST.Declarations {
    public sealed class EnumDeclarationNode : DeclarationNode {
        public EnumDeclarationNode(SourcePosition position, EnumNode @enum, string name, bool exported) : base(position) {
            Enum = @enum;
            Name = name;
            Exported = exported;
        }

        public EnumNode Enum { get; }

        public string Name { get; }

        public bool Exported { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitEnumDeclaration(this);
        }
    }
}
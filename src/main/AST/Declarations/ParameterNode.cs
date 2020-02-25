using Neo.AST.Expressions.Atoms;
using Neo.Frontend.Lexer;

namespace Neo.AST.Declarations {
    public sealed class ParameterNode : DeclarationNode {
        public ParameterNode(SourcePosition position, IdentNode name, bool frozen) : base(position) {
            Name = name;
            Frozen = frozen;
        }

        public IdentNode Name { get; }

        public bool Frozen { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitParameter(this);
        }
    }
}
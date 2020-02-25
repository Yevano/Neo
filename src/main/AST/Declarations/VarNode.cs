using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.Frontend.Lexer;

namespace Neo.AST.Declarations {
    public sealed class VarNode : DeclarationNode {
        public VarNode(SourcePosition position, IdentNode name, ExpressionNode defaultValue, bool exported, bool final) : base(position) {
            Name = name;
            DefaultValue = defaultValue;
            Exported = exported;
            Final = final;
        }

        public IdentNode Name { get; }

        public ExpressionNode DefaultValue { get; }

        public bool Exported { get; }

        public bool Final { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitVar(this);
        }
    }
}
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;
using Neo.Frontend.Lexer;
using System.Collections.Generic;

namespace Neo.AST.Declarations {
    public sealed class ProcNode : DeclarationNode, IProcNode {
        public ProcNode(SourcePosition position, IdentNode name, bool exported) : base(position) {
            Name = name;
            Exported = exported;
            Parameters = new List<ParameterNode>();
            Statements = new List<StatementNode>();
        }

        public IdentNode Name { get; }

        public bool Exported { get; }

        public bool Varargs { get; set; }

        public List<ParameterNode> Parameters { get; }

        public List<StatementNode> Statements { get; }

        string IProcNode.Name => Name.Value;

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitProc(this);
        }
    }
}
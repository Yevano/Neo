using Neo.AST.Declarations;
using Neo.AST.Statements;
using Neo.Frontend.Lexer;
using System.Collections.Generic;

namespace Neo.AST {
    public sealed class ChunkNode : Node {
        public ChunkNode(SourcePosition position, string name) : base(position) {
            Name = name;
            Procedures = new List<ProcNode>();
            Variables = new List<VarNode>();
            Enums = new List<EnumDeclarationNode>();
            Imports = new List<ImportNode>();
        }

        public string Name { get; }

        public List<ProcNode> Procedures { get; }

        public List<VarNode> Variables { get; }

        public List<EnumDeclarationNode> Enums { get; }

        public List<ImportNode> Imports { get; }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitChunk(this);
        }
    }
}
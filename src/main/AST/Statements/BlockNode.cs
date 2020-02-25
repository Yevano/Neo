using Neo.Frontend.Lexer;
using System.Collections.Generic;

namespace Neo.AST.Statements {
    public sealed class BlockNode : StatementNode {
        private readonly List<StatementNode> statements;

        public BlockNode(SourcePosition position) : base(position) {
            statements = new List<StatementNode>();
        }

        public IEnumerable<StatementNode> Statements {
            get {
                foreach(var statement in statements) {
                    yield return statement;
                }
            }
        }

        public void AddStatement(StatementNode statement) {
            statements.Add(statement);
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitBlock(this);
        }
    }
}
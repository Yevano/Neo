using Neo.AST.Declarations;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;
using Neo.Frontend.Lexer;
using System.Collections.Generic;

namespace Neo.AST.Expressions {
    public sealed class LambdaNode : ExpressionNode, IProcNode {
        private static int currentID;

        private readonly int id;

        public LambdaNode(SourcePosition position) : base(position) {
            id = currentID++;
            Parameters = new List<ParameterNode>();
            Statements = new List<StatementNode>();
        }

        public bool Varargs { get; set; }

        public List<ParameterNode> Parameters { get; }

        public List<StatementNode> Statements { get; }

        public string Name => $"lambda{id}";

        public override bool IsConstant() {
            return true;  
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitLambda(this);
        }
    }
}
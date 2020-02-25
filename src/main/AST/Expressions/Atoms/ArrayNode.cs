using Neo.Frontend.Lexer;
using System.Collections.Generic;
using System.Linq;

namespace Neo.AST.Expressions.Atoms {
    public sealed class ArrayNode : ExpressionNode {
        private readonly List<ExpressionNode> defaultElements;

        public ArrayNode(SourcePosition position) : base(position) {
            defaultElements = new List<ExpressionNode>();
        }

        public IEnumerable<ExpressionNode> DefaultElements {
            get {
                foreach(var element in defaultElements) {
                    yield return element;
                }
            }
        }

        public void AddDefaultElement(ExpressionNode element) {
            defaultElements.Add(element);
        }

        public override bool IsConstant() {
            return defaultElements.Aggregate(true, (acc, x) => acc && x.IsConstant());
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitArray(this);
        }
    }
}
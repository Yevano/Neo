using Neo.Frontend.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.AST.Expressions.Atoms {
    public sealed class ObjectNode : ExpressionNode {
        private readonly Dictionary<ExpressionNode, ExpressionNode> defaultElements;

        public ObjectNode(SourcePosition position) : base(position) {
            defaultElements = new Dictionary<ExpressionNode, ExpressionNode>();
        }

        public IEnumerable<Tuple<ExpressionNode, ExpressionNode>> DefaultElements {
            get {
                foreach (var mapping in defaultElements) {
                    yield return new Tuple<ExpressionNode, ExpressionNode>(mapping.Key, mapping.Value);
                }
            }
        }

        public void AddDefaultElement(ExpressionNode key, ExpressionNode value) {
            if (defaultElements.ContainsKey(key)) {
                throw new InvalidOperationException("key already contained!");
            }

            defaultElements[key] = value;
        }

        public override bool IsConstant() {
            return defaultElements.Aggregate(true, (acc, x) => acc && x.Key.IsConstant() && x.Value.IsConstant());
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitObject(this);
        }
    }
}
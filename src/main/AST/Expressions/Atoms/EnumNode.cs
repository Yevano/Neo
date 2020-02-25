using Neo.Frontend.Lexer;
using System.Collections.Generic;

namespace Neo.AST.Expressions.Atoms {
    public sealed class EnumNode : ExpressionNode {
        private readonly List<string> elements;

        public EnumNode(SourcePosition position) : base(position) {
            elements = new List<string>();
        }

        public IEnumerable<string> Elements {
            get {
                foreach (var element in elements) {
                    yield return element;
                }
            }
        }

        public void AddElement(string element) => elements.Add(element);

        public override bool IsConstant() {
            return true;
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitEnum(this);
        }
    }
}
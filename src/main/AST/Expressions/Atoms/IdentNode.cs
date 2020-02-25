using Neo.Frontend.Lexer;

namespace Neo.AST.Expressions.Atoms {
    public sealed class IdentNode : ExpressionNode {
        public IdentNode(SourcePosition position, string value) : base(position) {
            Value = value;
        }

        public string Value { get; }

        public override bool IsConstant() {
        	// @TODO: Not sure how far we can take this; we don't have any context here.
            // We'd need to be able to scan up the Scopes at "compile time" to check for
            // a known constant value, which would be difficult given how final variables
            // are implemented currently. Meh.
            //              - sci4me, Mar 31, 2019
            return false;
        }

        public override void Accept(IASTVisitor visitor) {
            visitor.VisitIdent(this);
        }
    }
}
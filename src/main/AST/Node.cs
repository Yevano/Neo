using Neo.Frontend.Lexer;

namespace Neo.AST {
    public abstract class Node : IASTVisitable {
        internal Node(SourcePosition position) {
        	Position = position;
        }

        public SourcePosition Position { get; }

        public abstract void Accept(IASTVisitor visitor);
    }
}
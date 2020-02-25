namespace Neo.AST {
    public interface IASTVisitable {
        void Accept(IASTVisitor visitor);
    }
}
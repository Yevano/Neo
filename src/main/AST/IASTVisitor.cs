using Neo.AST.Declarations;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.AST {
    public interface IASTVisitor {
        void VisitChunk(ChunkNode node);

        void VisitProc(ProcNode node);

        void VisitBreak(BreakNode node);

        void VisitContinue(ContinueNode node);

        void VisitThrow(ThrowNode node);

        void VisitImport(ImportNode node);

        void VisitParameter(ParameterNode node);

        void VisitReturn(ReturnNode node);

        void VisitDefer(DeferNode node);

        void VisitTernary(TernaryNode node);

        void VisitTryCatch(TryCatchNode node);

        void VisitEnum(EnumNode node);

        void VisitEnumDeclaration(EnumDeclarationNode node);

        void VisitSlice(SliceNode node);

        void VisitWhile(WhileNode node);

        void VisitExpressionStatement(ExpressionStatementNode node);

        void VisitVar(VarNode node);

        void VisitIf(IfNode node);

        void VisitDo(DoNode node);

        void VisitArray(ArrayNode node);

        void VisitParen(ParenNode node);

        void VisitString(StringNode node);

        void VisitForRange(ForRangeNode node);

        void VisitForIterator(ForIteratorNode node);

        void VisitForKeyValueIterator(ForKeyValueIteratorNode node);

        void VisitUnary(UnaryNode node);

        void VisitNil(NilNode node);

        void VisitAssign(AssignNode node);

        void VisitInt(IntNode node);

        void VisitIndex(IndexNode node);

        void VisitBlock(BlockNode node);

        void VisitIdent(IdentNode node);

        void VisitBinary(BinaryNode node);

        void VisitFloat(FloatNode node);

        void VisitBool(BoolNode node);

        void VisitCall(CallNode node);

        void VisitObject(ObjectNode node);

        void VisitLambda(LambdaNode node);

        void VisitVarargs(VarargsNode node);
    }
}
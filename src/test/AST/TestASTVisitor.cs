using System;

using Neo.AST;
using Neo.AST.Declarations;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST {
	public sealed class TestASTVisitor : IASTVisitor {
		public Action<ChunkNode> VisitChunkHandler { get; set; }
        public Action<ProcNode> VisitProcHandler { get; set; }
        public Action<BreakNode> VisitBreakHandler { get; set; }
        public Action<ContinueNode> VisitContinueHandler { get; set; }
        public Action<ThrowNode> VisitThrowHandler { get; set; }
        public Action<ImportNode> VisitImportHandler { get; set; }
        public Action<ParameterNode> VisitParameterHandler { get; set; }
        public Action<ReturnNode> VisitReturnHandler { get; set; }
        public Action<DeferNode> VisitDeferHandler { get; set; }
        public Action<TernaryNode> VisitTernaryHandler { get; set; }
        public Action<TryCatchNode> VisitTryCatchHandler { get; set; }
        public Action<EnumNode> VisitEnumHandler { get; set; }
        public Action<EnumDeclarationNode> VisitEnumDeclarationHandler { get; set; }
        public Action<SliceNode> VisitSliceHandler { get; set; }
        public Action<WhileNode> VisitWhileHandler { get; set; }
        public Action<ExpressionStatementNode> VisitExpressionStatementNodeHandler { get; set; }
        public Action<VarNode> VisitVarHandler { get; set; }
        public Action<IfNode> VisitIfHandler { get; set; }
        public Action<DoNode> VisitDoHandler { get; set; }
        public Action<ArrayNode> VisitArrayHandler { get; set; }
        public Action<ParenNode> VisitParenHandler { get; set; }
        public Action<StringNode> VisitStringHandler { get; set; }
        public Action<ForRangeNode> VisitForRangeHandler { get; set; }
        public Action<ForIteratorNode> VisitForIteratorHandler { get; set; }
        public Action<ForKeyValueIteratorNode> VisitForKeyValueIteratorHandler { get; set; }
        public Action<UnaryNode> VisitUnaryHandler { get; set; }
        public Action<NilNode> VisitNilHandler { get; set; }
        public Action<AssignNode> VisitAssignHandler { get; set; }
        public Action<IntNode> VisitIntHandler { get; set; }
        public Action<IndexNode> VisitIndexHandler { get; set; }
        public Action<BlockNode> VisitBlockHandler { get; set; }
        public Action<IdentNode> VisitIdentHandler { get; set; }
        public Action<BinaryNode> VisitBinaryHandler { get; set; }
        public Action<FloatNode> VisitFloatHandler { get; set; }
        public Action<BoolNode> VisitBoolHandler { get; set; }
        public Action<CallNode> VisitCallHandler { get; set; }
        public Action<ObjectNode> VisitObjectHandler { get; set; }
        public Action<LambdaNode> VisitLambdaHandler { get; set; }
        public Action<VarargsNode> VisitVarargsHandler { get; set; }

        public void VisitChunk(ChunkNode node) { VisitChunkHandler(node); }
		public void VisitProc(ProcNode node) { VisitProcHandler(node); }
		public void VisitBreak(BreakNode node) { VisitBreakHandler(node); }
		public void VisitContinue(ContinueNode node) { VisitContinueHandler(node); }
		public void VisitThrow(ThrowNode node) { VisitThrowHandler(node); }
		public void VisitImport(ImportNode node) { VisitImportHandler(node); }
		public void VisitParameter(ParameterNode node) { VisitParameterHandler(node); }
		public void VisitReturn(ReturnNode node) { VisitReturnHandler(node); }
		public void VisitDefer(DeferNode node) { VisitDeferHandler(node); }
		public void VisitTernary(TernaryNode node) { VisitTernaryHandler(node); }
		public void VisitTryCatch(TryCatchNode node) { VisitTryCatchHandler(node); }
		public void VisitEnum(EnumNode node) { VisitEnumHandler(node); }
		public void VisitEnumDeclaration(EnumDeclarationNode node) { VisitEnumDeclarationHandler(node); }
		public void VisitSlice(SliceNode node) { VisitSliceHandler(node); }
		public void VisitWhile(WhileNode node) { VisitWhileHandler(node); }
		public void VisitExpressionStatement(ExpressionStatementNode node) { VisitExpressionStatementNodeHandler(node); }
		public void VisitVar(VarNode node) { VisitVarHandler(node); }
		public void VisitIf(IfNode node) { VisitIfHandler(node); }
		public void VisitDo(DoNode node) { VisitDoHandler(node); }
		public void VisitArray(ArrayNode node) { VisitArrayHandler(node); }
		public void VisitParen(ParenNode node) { VisitParenHandler(node); }
		public void VisitString(StringNode node) { VisitStringHandler(node); }
		public void VisitForRange(ForRangeNode node) { VisitForRangeHandler(node); }
		public void VisitForIterator(ForIteratorNode node) { VisitForIteratorHandler(node); }
		public void VisitForKeyValueIterator(ForKeyValueIteratorNode node) { VisitForKeyValueIteratorHandler(node); }
		public void VisitUnary(UnaryNode node) { VisitUnaryHandler(node); }
		public void VisitNil(NilNode node) { VisitNilHandler(node); }
		public void VisitAssign(AssignNode node) { VisitAssignHandler(node); }
		public void VisitInt(IntNode node) { VisitIntHandler(node); }
		public void VisitIndex(IndexNode node) { VisitIndexHandler(node); }
		public void VisitBlock(BlockNode node) { VisitBlockHandler(node); }
		public void VisitIdent(IdentNode node) { VisitIdentHandler(node); }
		public void VisitBinary(BinaryNode node) { VisitBinaryHandler(node); }
		public void VisitFloat(FloatNode node) { VisitFloatHandler(node); }
		public void VisitBool(BoolNode node) { VisitBoolHandler(node); }
		public void VisitCall(CallNode node) { VisitCallHandler(node); }
		public void VisitObject(ObjectNode node) { VisitObjectHandler(node); }
		public void VisitLambda(LambdaNode node) { VisitLambdaHandler(node); }
		public void VisitVarargs(VarargsNode node) { VisitVarargsHandler(node); }
	}
}
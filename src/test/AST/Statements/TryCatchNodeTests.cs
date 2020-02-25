using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class TryCatchNodeTests {
		private StatementNode @try;
		private StatementNode @catch;
		private TryCatchNode subject;

		public TryCatchNodeTests() {
			@try = new BlockNode(SourcePosition.NIL);
			@catch = new BlockNode(SourcePosition.NIL);
			subject = new TryCatchNode(SourcePosition.NIL, @try, @catch, new IdentNode(SourcePosition.NIL, "foo"));
		}

		[Test]
		public void TryCatchNodesHaveAMonotonicallyIncrementingID() {
			var first = new TryCatchNode(SourcePosition.NIL, new BlockNode(SourcePosition.NIL), new BlockNode(SourcePosition.NIL), new IdentNode(SourcePosition.NIL, "foo"));
			var second = new TryCatchNode(SourcePosition.NIL, new BlockNode(SourcePosition.NIL), new BlockNode(SourcePosition.NIL), new IdentNode(SourcePosition.NIL, "foo"));
			var third = new TryCatchNode(SourcePosition.NIL, new BlockNode(SourcePosition.NIL), new BlockNode(SourcePosition.NIL), new IdentNode(SourcePosition.NIL, "foo"));
		
			Assert.AreEqual(second.ID, first.ID + 1);
			Assert.AreEqual(third.ID, second.ID + 1);
		}

		[Test]
		public void TryCatchNodeHasATryStatement() {
			Assert.AreEqual(subject.Try, @try);
		}

		[Test]
		public void TryCatchNodeHasACatchStatement() {
			Assert.AreEqual(subject.Catch, @catch);
		}

		[Test]
		public void TryCatchNodeHasAnErrorIdentifier() {
			Assert.AreEqual(subject.Error.Value, "foo");
		}

		[Test]
		public void TryCatchNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitTryCatchHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class DoNodeTests {
		private ExpressionNode condition;
		private StatementNode code;
		private DoNode subject;

		public DoNodeTests() {
			condition = new IdentNode(SourcePosition.NIL, "foo");
			code = new BlockNode(SourcePosition.NIL);
			subject = new DoNode(SourcePosition.NIL, condition, code);
		}

		[Test]
		public void DoNodeHasACondition() {
			Assert.AreEqual(subject.Condition, condition);
		}

		[Test]
		public void DoNodeHasAStatement() {
			Assert.AreEqual(subject.Code, code);
		}

		[Test]
		public void DoNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitDoHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class IfNodeTests {
		private ExpressionNode condition;
		private StatementNode @true;
		private StatementNode @false;
		private IfNode subject;

		public IfNodeTests() {
			condition = new IdentNode(SourcePosition.NIL, "foo");
			@true = new BlockNode(SourcePosition.NIL);
			@false = new BlockNode(SourcePosition.NIL);
			subject = new IfNode(SourcePosition.NIL, condition, @true, @false);
		}

		[Test]
		public void IfNodeHasACondition() {
			Assert.AreEqual(subject.Condition, condition);
		}

		[Test]
		public void IfNodeHasATrueStatement() {
			Assert.AreEqual(subject.True, @true);
		}

		[Test]
		public void IfNodeHasAFalseStatement() {
			Assert.AreEqual(subject.False, @false);
		}

		[Test]
		public void IfNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitIfHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class ExpressionStatementNodeTests {
		private ExpressionNode value;
		private ExpressionStatementNode subject;

		public ExpressionStatementNodeTests() {
			value = new IdentNode(SourcePosition.NIL, "foo");
			subject = new ExpressionStatementNode(SourcePosition.NIL, value);
		}

		[Test]
		public void ExpressionStatementNodeHasAValue() {
			Assert.AreEqual(subject.Value, value);
		}

		[Test]
		public void ExpressionStatementNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitExpressionStatementNodeHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class WhileNodeTests {
		private ExpressionNode condition;
		private StatementNode code;
		private WhileNode subject;

		public WhileNodeTests() {
			condition = new IdentNode(SourcePosition.NIL, "foo");
			code = new BlockNode(SourcePosition.NIL);
			subject = new WhileNode(SourcePosition.NIL, condition, code);
		}

		[Test]
		public void WhileNodeHasACondition() {
			Assert.AreEqual(subject.Condition, condition);
		}

		[Test]
		public void WhileNodeHasAStatement() {
			Assert.AreEqual(subject.Code, code);
		}

		[Test]
		public void WhileNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitWhileHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
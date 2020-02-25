using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class AssignNodeTests {
		private ExpressionNode left;
		private ExpressionNode right;
		private AssignNode subject;

		public AssignNodeTests() {
			left = new IdentNode(SourcePosition.NIL, "level");
			right = new IntNode(SourcePosition.NIL, 9000);
			subject = new AssignNode(SourcePosition.NIL, AssignOP.NORMAL, left, right);
		}

		[Test]
		public void AssignNodeHasAnAssignOP() {
			Assert.AreEqual(subject.OP, AssignOP.NORMAL);
		}

		[Test]
		public void AssignNodeHasALeftSide() {
			Assert.AreEqual(subject.Left, left);
		}

		[Test]
		public void AssignNodeHasARightSide() {
			Assert.AreEqual(subject.Right, right);
		}

		[Test]
		public void AssignNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitAssignHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions {
	[TestFixture]
	public sealed class TernaryNodeTests {
		private TernaryNode subject;

		public TernaryNodeTests() {
			subject = new TernaryNode(new SourcePosition(2, 4), new IdentNode(new SourcePosition(2, 5), "foo"), new IntNode(new SourcePosition(2, 6), 42), new IntNode(new SourcePosition(2, 6), 21));
		}

		[Test]
		public void TernaryNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void TernaryNodesHaveAConditionExpression() {
			Assert.AreEqual(((IdentNode) subject.Condition).Value, "foo");
		}

		[Test]
		public void TernaryNodesHaveATrueExpression() {
			Assert.AreEqual(((IntNode) subject.True).Value, 42);
		}

		[Test]
		public void TernaryNodesHaveAFalseExpression() {
			Assert.AreEqual(((IntNode) subject.False).Value, 21);
		}

		[Test]
		public void TernaryNodesAreNotConstantIfTheConditionTrueAndFalseAreNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void TernaryNodesAreConstantIfTheConditionTrueAndFalseAreConstant() {
			subject = new TernaryNode(new SourcePosition(2, 4), new BoolNode(new SourcePosition(2, 5), true), new IntNode(new SourcePosition(2, 6), 42), new IntNode(new SourcePosition(2, 6), 21));

			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void TernaryNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitTernaryHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions {
	[TestFixture]
	public sealed class BinaryNodeTests {
		private BinaryNode subject;

		public BinaryNodeTests() {
			subject = new BinaryNode(new SourcePosition(2, 4), BinaryOP.CONCAT, new IdentNode(new SourcePosition(2, 5), "foo"), new IdentNode(new SourcePosition(2, 5), "bar"));
		}

		[Test]
		public void BinaryNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void BinaryNodesHaveABinaryOP() {
			Assert.AreEqual(subject.OP, BinaryOP.CONCAT);
		}

		[Test]
		public void BinaryNodesHaveALeftValue() {
			Assert.AreEqual(((IdentNode) subject.Left).Value, "foo");
		}

		[Test]
		public void BinaryNodesHaveARightValue() {
			Assert.AreEqual(((IdentNode) subject.Right).Value, "bar");
		}

		[Test]
		public void BinaryNodesAreNotConstantIfBothSidesAreNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void BinaryNodesAreConstantIfBothSidesAreConstant() {
			subject = new BinaryNode(new SourcePosition(2, 4), BinaryOP.ADD, new IntNode(new SourcePosition(2, 5), 42), new IntNode(new SourcePosition(2, 5), 21));

			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void BinaryNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitBinaryHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
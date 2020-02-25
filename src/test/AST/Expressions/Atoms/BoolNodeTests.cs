using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class BoolNodeTests {
		private BoolNode subject;

		public BoolNodeTests() {
			subject = new BoolNode(new SourcePosition(2, 4), true);
		}

		[Test]
		public void BoolNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void BoolNodesHaveAValue() {
			Assert.AreEqual(subject.Value, true);
		}

		[Test]
		public void BoolNodesAreConstant() {
			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void BoolNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitBoolHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
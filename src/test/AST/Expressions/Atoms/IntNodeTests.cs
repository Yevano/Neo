using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class IntNodeTests {
		private IntNode subject;

		public IntNodeTests() {
			subject = new IntNode(new SourcePosition(2, 4), 42);
		}
		
		[Test]
		public void IntNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void IntNodesHaveAValue() {
			Assert.AreEqual(subject.Value, 42);
		}

		[Test]
		public void IntNodesAreConstant() {
			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void IntNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitIntHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
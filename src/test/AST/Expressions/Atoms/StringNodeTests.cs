using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class StringNodeTests {
		private StringNode subject;

		public StringNodeTests() {
			subject = new StringNode(new SourcePosition(2, 4), "foo");
		}
		
		[Test]
		public void StringNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void StringNodesHaveAValue() {
			Assert.AreEqual(subject.Value, "foo");
		}

		[Test]
		public void StringNodesAreConstant() {
			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void StringNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitStringHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
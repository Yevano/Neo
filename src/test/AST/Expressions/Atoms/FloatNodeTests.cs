using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class FloatNodeTests {
		private FloatNode subject;

		public FloatNodeTests() {
			subject = new FloatNode(new SourcePosition(2, 4), 3.14);
		}

		[Test]
		public void FloatNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void FloatNodesHaveAValue() {
			Assert.AreEqual(subject.Value, 3.14);
		}

		[Test]
		public void FloatNodesAreConstant() {
			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void FloatNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitFloatHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
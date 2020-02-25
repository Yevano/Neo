using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Declarations;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Declarations {
	[TestFixture]
	public sealed class ParameterNodeTests {
		private ParameterNode subject;

		public ParameterNodeTests() {
			subject = new ParameterNode(new SourcePosition(2, 4), new IdentNode(SourcePosition.NIL, "foo"), true);
		}

		[Test]
		public void ParameterNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void ParameterNodesHaveAName() {
			Assert.AreEqual(subject.Name.Value, "foo");
		}

		[Test]
		public void ParameterNodesHaveAFrozenFlag() {
			Assert.AreEqual(subject.Frozen, true);
		}

		[Test]
		public void ParameterNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitParameterHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class IdentNodeTests {
		private IdentNode subject;

		public IdentNodeTests() {
			subject = new IdentNode(new SourcePosition(2, 4), "foo");
		}
		
		[Test]
		public void IdentNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void IdentNodesHaveAValue() {
			Assert.AreEqual(subject.Value, "foo");
		}

		[Test]
		public void IdentNodesAreNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void IdentNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitIdentHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
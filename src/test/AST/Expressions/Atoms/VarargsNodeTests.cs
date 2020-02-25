using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class VarargsNodeTests {
		private VarargsNode subject;

		public VarargsNodeTests() {
			subject = new VarargsNode(new SourcePosition(2, 4));
		}
		
		[Test]
		public void VarargsNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void VarargsNodesAreNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void VarargsNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitVarargsHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class ThrowNodeTests {
		private ThrowNode subject;

		public ThrowNodeTests() {
			subject = new ThrowNode(SourcePosition.NIL, new StringNode(SourcePosition.NIL, "foo"));
		}

		[Test]
		public void ThrowNodeHasAValue() {
			Assert.AreEqual(((StringNode) subject.Value).Value, "foo");
		}

		[Test]
		public void ThrowNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitThrowHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
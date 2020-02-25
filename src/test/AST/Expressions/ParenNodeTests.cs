using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions {
	[TestFixture]
	public sealed class ParenNodeTests {
		private ParenNode subject;

		public ParenNodeTests() {
			subject = new ParenNode(new SourcePosition(2, 4), new IdentNode(new SourcePosition(2, 5), "foo"));
		}

		[Test]
		public void ParenNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void ParenNodesHaveAValue() {
			Assert.AreEqual(((IdentNode) subject.Value).Value, "foo");
		}

		[Test]
		public void ParenNodesAreNotConstantIfTheValueIsNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void ParenNodesAreConstantIfTheValueIsConstant() {
			subject = new ParenNode(new SourcePosition(2, 4), new StringNode(new SourcePosition(2, 5), "foo"));

			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void ParenNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitParenHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
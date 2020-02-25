using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions {
	[TestFixture]
	public sealed class UnaryNodeTests {
		private UnaryNode subject;

		public UnaryNodeTests() {
			subject = new UnaryNode(new SourcePosition(2, 4), UnaryOP.BIT_NOT, new IdentNode(new SourcePosition(2, 5), "foo"));
		}

		[Test]
		public void UnaryNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void UnaryNodesHaveAUnaryOP() {
			Assert.AreEqual(subject.OP, UnaryOP.BIT_NOT);
		}

		[Test]
		public void UnaryNodesHaveAValue() {
			Assert.AreEqual(((IdentNode) subject.Value).Value, "foo");
		}

		[Test]
		public void UnaryNodesAreNotConstantIfTheValueIsNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void UnaryNodesAreonstantIfTheValueIsConstant() {
			subject = new UnaryNode(new SourcePosition(2, 4), UnaryOP.BIT_NOT, new IntNode(new SourcePosition(2, 5), 42));

			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void UnaryNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitUnaryHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
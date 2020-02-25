using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions {
	[TestFixture]
	public sealed class IndexNodeTests {
		private IndexNode subject;

		public IndexNodeTests() {
			subject = new IndexNode(new SourcePosition(2, 4), new IdentNode(new SourcePosition(2, 5), "foo"), new IntNode(new SourcePosition(2, 6), 42));
		}
		
		[Test]
		public void IndexNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void IndexNodesHaveAValue() {
			Assert.AreEqual(((IdentNode) subject.Value).Value, "foo");
		}

		[Test]
		public void IndexNodesHaveAKey() {
			Assert.AreEqual(((IntNode) subject.Key).Value, 42);
		}

		[Test]
		public void IndexNodesAreNotConstantIfTheValueAndKeyAreNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void IndexNodesAreConstantIfTheValueAndKeyAreConstant() {
			subject = new IndexNode(new SourcePosition(2, 4), new StringNode(new SourcePosition(2, 5), "foo"), new IntNode(new SourcePosition(2, 6), 42));
			
			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void IndexNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitIndexHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
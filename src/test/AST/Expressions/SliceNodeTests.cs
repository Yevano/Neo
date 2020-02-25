using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions {
	[TestFixture]
	public sealed class SliceNodeTests {
		private SliceNode subject;

		public SliceNodeTests() {
			subject = new SliceNode(new SourcePosition(2, 4), new IdentNode(new SourcePosition(2, 5), "foo"), new IntNode(new SourcePosition(2, 6), 42), new IntNode(new SourcePosition(2, 6), 21));
		}

		[Test]
		public void SliceNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void SliceNodesHaveAnArrayExpression() {
			Assert.AreEqual(((IdentNode) subject.Array).Value, "foo");
		}

		[Test]
		public void SliceNodesHaveAStartExpression() {
			Assert.AreEqual(((IntNode) subject.Start).Value, 42);
		}

		[Test]
		public void SliceNodesHaveAEndExpression() {
			Assert.AreEqual(((IntNode) subject.End).Value, 21);
		}

		[Test]
		public void SliceNodesAreNotConstantIfTheArrayStartAndEndAreNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void SliceNodesAreConstantIfTheArrayStartAndEndAreConstant() {
			subject = new SliceNode(new SourcePosition(2, 4), new StringNode(new SourcePosition(2, 5), "foo"), new IntNode(new SourcePosition(2, 6), 1), new IntNode(new SourcePosition(2, 6), 2));

			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void SliceNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitSliceHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
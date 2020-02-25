using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions {
	[TestFixture]
	public sealed class CallNodeTests {
		private CallNode subject;

		public CallNodeTests() {
			subject = new CallNode(new SourcePosition(2, 4), new IdentNode(SourcePosition.NIL, "foo"));
		}

		[Test]
		public void CallNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void CallNodesHaveAProc() {
			Assert.AreEqual(((IdentNode) subject.Proc).Value, "foo");
		}

		[Test]
		public void CallNodesHaveArguments() {
			subject.Arguments.Add(new IntNode(SourcePosition.NIL, 42));
			subject.Arguments.Add(new StringNode(SourcePosition.NIL, "let me out"));

			Assert.AreEqual(subject.Arguments.Count, 2);
			Assert.AreEqual(((IntNode) subject.Arguments[0]).Value, 42);
			Assert.AreEqual(((StringNode) subject.Arguments[1]).Value, "let me out");
		}

		[Test]
		public void CallNodesAreNotConstant() {
			Assert.AreEqual(subject.IsConstant(), false);
		}

		[Test]
		public void CallNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitCallHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
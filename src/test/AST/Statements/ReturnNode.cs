using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class ReturnNodeTests {
		private ReturnNode subject;

		public ReturnNodeTests() {
			subject = new ReturnNode(SourcePosition.NIL, new StringNode(SourcePosition.NIL, "foo"));
		}

		[Test]
		public void ReturnNodeHasAValue() {
			Assert.AreEqual(((StringNode) subject.Value).Value, "foo");
		}

		[Test]
		public void ReturnNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitReturnHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
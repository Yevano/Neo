using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class ForKeyValueIteratorNodeTests {
		private ExpressionNode from;
		private StatementNode code;
		private ForKeyValueIteratorNode subject;

		public ForKeyValueIteratorNodeTests() {
			from = new IdentNode(SourcePosition.NIL, "foo");
			code = new BlockNode(SourcePosition.NIL);
			subject = new ForKeyValueIteratorNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "k"), new IdentNode(SourcePosition.NIL, "v"), from, code);
		}

		[Test]
		public void ForKeyValueIteratorNodeHasAKeyIterator() {
			Assert.AreEqual(subject.Key.Value, "k");
		}

		[Test]
		public void ForKeyValueIteratorNodeHasAValueIterator() {
			Assert.AreEqual(subject.Value.Value, "v");
		}

		[Test]
		public void ForKeyValueIteratorNodeHasAFromExpression() {
			Assert.AreEqual(subject.From, from);
		}

		[Test]
		public void ForKeyValueIteratorNodeHasAStatement() {
			Assert.AreEqual(subject.Code, code);
		}

		[Test]
		public void ForKeyValueIteratorNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitForKeyValueIteratorHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
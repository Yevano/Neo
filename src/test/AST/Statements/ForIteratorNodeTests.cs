using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class ForIteratorNodeTests {
		private ExpressionNode from;
		private StatementNode code;
		private ForIteratorNode subject;

		public ForIteratorNodeTests() {
			from = new IdentNode(SourcePosition.NIL, "foo");
			code = new BlockNode(SourcePosition.NIL);
			subject = new ForIteratorNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "i"), from, code);
		}

		[Test]
		public void ForIteratorNodeHasAnIterator() {
			Assert.AreEqual(subject.Iterator.Value, "i");
		}

		[Test]
		public void ForIteratorNodeHasAFromExpression() {
			Assert.AreEqual(subject.From, from);
		}

		[Test]
		public void ForIteratorNodeHasAStatement() {
			Assert.AreEqual(subject.Code, code);
		}

		[Test]
		public void ForIteratorNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitForIteratorHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
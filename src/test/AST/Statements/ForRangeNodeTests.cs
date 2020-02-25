using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class ForRangeNodeTests {
		private ExpressionNode start;
		private ExpressionNode end;
		private ExpressionNode by;
		private StatementNode code;
		private ForRangeNode subject;

		public ForRangeNodeTests() {
			start = new IntNode(SourcePosition.NIL, 0);
			end = new IntNode(SourcePosition.NIL, 100);
			by = new IntNode(SourcePosition.NIL, 3);
			code = new BlockNode(SourcePosition.NIL);
			subject = new ForRangeNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "i"), start, end, by, code);
		}

		[Test]
		public void ForRangeNodeHasAnIterator() {
			Assert.AreEqual(subject.Iterator.Value, "i");
		}

		[Test]
		public void ForRangeNodeHasAStartExpression() {
			Assert.AreEqual(subject.Start, start);
		}

		[Test]
		public void ForRangeNodeHasAnEndExpression() {
			Assert.AreEqual(subject.End, end);
		}

		[Test]
		public void ForRangeNodeHasAByExpression() {
			Assert.AreEqual(subject.By, by);
		}

		[Test]
		public void ForRangeNodeHasAStatement() {
			Assert.AreEqual(subject.Code, code);
		}

		[Test]
		public void ForRangeNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitForRangeHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
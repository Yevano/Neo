using System.Linq;
using System.Collections.Generic;

using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class DeferNodeTests {
		private StatementNode code;
		private DeferNode subject;

		public DeferNodeTests() {
			code = new BlockNode(SourcePosition.NIL);
			subject = new DeferNode(SourcePosition.NIL, code);
		}

		[Test]
		public void DeferNodesHaveAMonotonicallyIncrementingID() {
			var first = new DeferNode(SourcePosition.NIL, new BlockNode(SourcePosition.NIL));
			var second = new DeferNode(SourcePosition.NIL, new BlockNode(SourcePosition.NIL));
			var third = new DeferNode(SourcePosition.NIL, new BlockNode(SourcePosition.NIL));
		
			Assert.AreEqual(second.ID, first.ID + 1);
			Assert.AreEqual(third.ID, second.ID + 1);
		}

		[Test]
		public void DeferNodesHaveAStatement() {
			Assert.AreEqual(subject.Code, code);
		}

		[Test]
		public void DeferNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitDeferHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
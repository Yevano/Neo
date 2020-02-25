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
	public sealed class ContinueNodeTests {
		private ContinueNode subject;

		public ContinueNodeTests() {
			subject = new ContinueNode(SourcePosition.NIL);
		}

		[Test]
		public void ContinueNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitContinueHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
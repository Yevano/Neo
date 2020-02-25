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
	public sealed class BlockNodeTests {
		private BlockNode subject;

		public BlockNodeTests() {
			subject = new BlockNode(SourcePosition.NIL);
		}

		[Test]
		public void BlockNodesHaveStatements() {
			var statements = new List<StatementNode>();
			statements.Add(new BreakNode(SourcePosition.NIL));
			statements.Add(new ContinueNode(SourcePosition.NIL));
			statements.Add(new ReturnNode(SourcePosition.NIL, new NilNode(SourcePosition.NIL)));
			
			for(var i = 0; i < statements.Count; i++) {
				subject.AddStatement(statements[i]);
			}

			var subjectStatements = subject.Statements.ToList();

			Assert.AreEqual(subjectStatements.Count, statements.Count);
			for(var i = 0; i < subjectStatements.Count; i++) {
				Assert.AreEqual(subjectStatements[i], statements[i]);
			}
		}

		[Test]
		public void BlockNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitBlockHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
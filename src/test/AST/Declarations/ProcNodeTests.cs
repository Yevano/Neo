using System.Collections.Generic;

using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Declarations;
using Neo.AST.Statements;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Declarations {
	[TestFixture]
	public sealed class ProcNodeTests {
		private ProcNode subject;

		public ProcNodeTests() {
			subject = new ProcNode(new SourcePosition(2, 4), new IdentNode(SourcePosition.NIL, "foo"), true);
		}

		[Test]
		public void ProcNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void ProcNodesHaveAName() {
			Assert.AreEqual(subject.Name.Value, "foo");
			Assert.AreEqual(((IProcNode) subject).Name, "foo");
		}

		[Test]
		public void ProcNodesHaveAnExportedFlag() {
			Assert.AreEqual(subject.Exported, true);
		}

		[Test]
		public void ProcNodesHaveParameters() {
			subject.Parameters.Add(new ParameterNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "blah"), true));
			subject.Parameters.Add(new ParameterNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "halb"), false));

			Assert.AreEqual(subject.Parameters[0].Name.Value, "blah");
			Assert.AreEqual(subject.Parameters[0].Frozen, true);
			Assert.AreEqual(subject.Parameters[1].Name.Value, "halb");
			Assert.AreEqual(subject.Parameters[1].Frozen, false);

			var asIProcNode = (IProcNode) subject;

			Assert.AreEqual(asIProcNode.Parameters[0].Name.Value, "blah");
			Assert.AreEqual(asIProcNode.Parameters[0].Frozen, true);
			Assert.AreEqual(asIProcNode.Parameters[1].Name.Value, "halb");
			Assert.AreEqual(asIProcNode.Parameters[1].Frozen, false);			
		}

		[Test]
		public void ProcNodesHaveStatements() {
			var statements = new List<StatementNode>();
			statements.Add(new BreakNode(SourcePosition.NIL));
			statements.Add(new ContinueNode(SourcePosition.NIL));
			statements.Add(new ReturnNode(SourcePosition.NIL, new NilNode(SourcePosition.NIL)));
			
			for(var i = 0; i < statements.Count; i++) {
				subject.Statements.Add(statements[i]);
			}

			var asIProcNode = (IProcNode) subject;

			Assert.AreEqual(subject.Statements.Count, statements.Count);
			Assert.AreEqual(asIProcNode.Statements.Count, statements.Count);
			for(var i = 0; i < subject.Statements.Count; i++) {
				Assert.AreEqual(subject.Statements[i], statements[i]);
				Assert.AreEqual(asIProcNode.Statements[i], statements[i]);
			}
		}

		[Test]
		public void ProcNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitProcHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
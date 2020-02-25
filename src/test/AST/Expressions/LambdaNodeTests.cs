using System.Collections.Generic;

using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Declarations;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Expressions {
	[TestFixture]
	public sealed class LambdaNodeTests {
		private LambdaNode subject;

		public LambdaNodeTests() {
			subject = new LambdaNode(SourcePosition.NIL);
		}

		[Test]
		public void LambdaNodesHaveUniqueNames() {
			const int N = 10;

			var set = new HashSet<string>();
			for(var i = 0; i < N; i++) {
				set.Add(new LambdaNode(new SourcePosition(i, 0)).Name);
			}

			Assert.AreEqual(set.Count, N);

			set.Clear();

			var asIProcNode = (IProcNode) subject;

			for(var i = 0; i < N; i++) {
				set.Add(((IProcNode) new LambdaNode(new SourcePosition(i, 0))).Name);
			}

			Assert.AreEqual(set.Count, N);
		}

		[Test]
		public void LambdaNodesHaveParameters() {
			subject.Parameters.Add(new ParameterNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "blah"), false));
			subject.Parameters.Add(new ParameterNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "yeet"), true));

			Assert.AreEqual(subject.Parameters[0].Name.Value, "blah");
			Assert.AreEqual(subject.Parameters[0].Frozen, false);
			Assert.AreEqual(subject.Parameters[1].Name.Value, "yeet");
			Assert.AreEqual(subject.Parameters[1].Frozen, true);

			var asIProcNode = (IProcNode) subject;

			Assert.AreEqual(asIProcNode.Parameters[0].Name.Value, "blah");
			Assert.AreEqual(asIProcNode.Parameters[0].Frozen, false);
			Assert.AreEqual(asIProcNode.Parameters[1].Name.Value, "yeet");
			Assert.AreEqual(asIProcNode.Parameters[1].Frozen, true);
		}

		[Test]
		public void LambdaNodesHaveStatements() {
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
		public void LambdaNodesAreConstant() {
			Assert.AreEqual(subject.IsConstant(), true);
		}

		[Test]
		public void LambdaNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitLambdaHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
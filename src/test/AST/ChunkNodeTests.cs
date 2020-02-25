using System.Collections.Generic;

using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Declarations;
using Neo.AST.Statements;

namespace Neo.Tests.AST {
	[TestFixture]
	public sealed class ChunkNodeTests {
		private ChunkNode subject;

		public ChunkNodeTests() {
			subject = new ChunkNode(SourcePosition.NIL, "foo");
		}

		[Test]
		public void ChunkNodesHaveAName() {
			Assert.AreEqual(subject.Name, "foo");
		}

		[Test]
		public void ChunkNodesHaveProcedures() {
			var procedures = new List<ProcNode>();
			for(var i = 0; i < 5; i++) {
				procedures.Add(new ProcNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "foo" + i), true));
			}

			foreach(var proc in procedures) {
				subject.Procedures.Add(proc);
			}

			for(var i = 0; i < subject.Procedures.Count; i++) {
				Assert.AreEqual(subject.Procedures[i], procedures[i]);
			}
		}

		[Test]
		public void ChunkNodesHaveVariables() {
			var variables = new List<VarNode>();
			for(var i = 0; i < 5; i++) {
				variables.Add(new VarNode(SourcePosition.NIL, new IdentNode(SourcePosition.NIL, "foo" + i), new IntNode(SourcePosition.NIL, 42), true, true));
			}

			foreach(var var in variables) {
				subject.Variables.Add(var);
			}

			for(var i = 0; i < subject.Variables.Count; i++) {
				Assert.AreEqual(subject.Variables[i], variables[i]);
			}
		}

		[Test]
		public void ChunkNodesHaveEnums() {
			var enums = new List<EnumDeclarationNode>();
			for(var i = 0; i < 5; i++) {
				var @enum = new EnumNode(SourcePosition.NIL);
				@enum.AddElement("reee");
				enums.Add(new EnumDeclarationNode(SourcePosition.NIL, @enum, "yeeee", true));
			}

			foreach(var @enumDeclaration in enums) {
				subject.Enums.Add(@enumDeclaration);
			}

			for(var i = 0; i < subject.Enums.Count; i++) {
				Assert.AreEqual(subject.Enums[i], @enums[i]);
			}
		}

		[Test]
		public void ChunkNodesHaveImports() {
			var imports = new List<ImportNode>();
			for(var i = 0; i < 5; i++) {
				var import = new ImportNode(SourcePosition.NIL, new StringNode(SourcePosition.NIL, "foo/bar" + i));
				import.Alias = new IdentNode(SourcePosition.NIL, "bar" + i);
				imports.Add(import);
			}

			foreach(var import in imports) {
				subject.Imports.Add(import);
			}

			for(var i = 0; i < subject.Imports.Count; i++) {
				Assert.AreEqual(subject.Imports[i], imports[i]);
			}
		}

		[Test]
		public void ChunkNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitChunkHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
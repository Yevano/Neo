using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;

namespace Neo.Tests.AST.Statements {
	[TestFixture]
	public sealed class ImportNodeTests {
		private ImportNode subject;

		public ImportNodeTests() {
			subject = new ImportNode(SourcePosition.NIL, new StringNode(SourcePosition.NIL, "foo/bar"));
			subject.Alias = new IdentNode(SourcePosition.NIL, "bar");
		}

		[Test]
		public void ImportNodeHasAPath() {
			Assert.AreEqual(subject.Path.Value, "foo/bar");
		}

		[Test]
		public void ImportNodeHasAnAlias() {
			Assert.AreEqual(subject.Alias.Value, "bar");
		}

		[Test]
		public void ImportNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitImportHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
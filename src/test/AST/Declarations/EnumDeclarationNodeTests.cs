using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST;
using Neo.AST.Declarations;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Declarations {
	[TestFixture]
	public sealed class EnumDeclarationNodeTests {
		private EnumNode @enum;
		private EnumDeclarationNode subject;

		public EnumDeclarationNodeTests() {
			@enum = new EnumNode(SourcePosition.NIL); 
			subject = new EnumDeclarationNode(new SourcePosition(2, 4), @enum, "SomeEnum", true);
		}
		
		[Test]
		public void EnumDeclarationNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void EnumDeclarationNodesHaveAnEnum() {
			Assert.AreEqual(subject.Enum, @enum);
		}

		[Test]
		public void EnumDeclarationNodesHaveAName() {
			Assert.AreEqual(subject.Name, "SomeEnum");
		}

		[Test]
		public void EnumDeclarationNodesHaveAnExportedFlag() {
			Assert.AreEqual(subject.Exported, true);
		}

		[Test]
		public void EnumDeclarationNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitEnumDeclarationHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
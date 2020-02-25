using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Declarations;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Declarations {
	[TestFixture]
	public sealed class VarNodeTests {
		private IntNode defaultValue;
		private VarNode subject;

		public VarNodeTests() {
			defaultValue = new IntNode(SourcePosition.NIL, 42);
			subject = new VarNode(new SourcePosition(2, 4), new IdentNode(SourcePosition.NIL, "foo"), defaultValue, true, true);
		}

		[Test]
		public void VarNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void VarNodesHaveAName() {
			Assert.AreEqual(subject.Name.Value, "foo");
		}

		[Test]
		public void VarNodesHaveADefaultValue() {
			Assert.AreEqual(subject.DefaultValue, defaultValue);
		}

		[Test]
		public void VarNodesHaveAnExportedFlag() {
			Assert.AreEqual(subject.Exported, true);
		}

		[Test]
		public void VarNodesHaveAFinalFlag() {
			Assert.AreEqual(subject.Final, true);
		}

		[Test]
		public void VarNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitVarHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
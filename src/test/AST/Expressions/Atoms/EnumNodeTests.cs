using System.Linq;

using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class EnumNodeTests {
		private EnumNode subject;

		public EnumNodeTests() {
			subject = new EnumNode(new SourcePosition(2, 4));
		}
		
		[Test]
		public void EnumNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void EnumNodesHaveElements() {
			subject.AddElement("foo");
			subject.AddElement("bar");

			var Elements = subject.Elements.ToList();
			Assert.AreEqual(Elements.Count, 2);
			Assert.AreEqual(Elements[0], "foo");
			Assert.AreEqual(Elements[1], "bar");
		}

		[Test]
		public void EnumNodesAreConstant() {
			Assert.AreEqual(subject.IsConstant(), true);	
		}

		[Test]
		public void EnumNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitEnumHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
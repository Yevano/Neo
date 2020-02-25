using System.Linq;

using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class ArrayNodeTests {
		private ArrayNode subject;

		public ArrayNodeTests() {
			subject = new ArrayNode(new SourcePosition(2, 4));
		}
		
		[Test]
		public void ArrayNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void ArrayNodesHaveDefaultElements() {
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "foo"));
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "bar"));

			var defaultElements = subject.DefaultElements.ToList();
			Assert.AreEqual(defaultElements.Count, 2);
			Assert.AreEqual(((StringNode) defaultElements[0]).Value, "foo");
			Assert.AreEqual(((StringNode) defaultElements[1]).Value, "bar");
		}

		[Test]
		public void ArrayNodesAreNotConstantIfTheyContainANonConstantElement() {
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "foo"));
			subject.AddDefaultElement(new IdentNode(SourcePosition.NIL, "bar"));
	
			Assert.AreEqual(subject.IsConstant(), false);			
		}

		[Test]
		public void ArrayNodesAreConstantIfTheyAreEmpty() {
			Assert.AreEqual(subject.IsConstant(), true);	
		}

		[Test]
		public void ArrayNodesAreConstantIfTheyOnlyContainConstantElements() {
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "foo"));
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "bar"));
	
			Assert.AreEqual(subject.IsConstant(), true);	
		}

		[Test]
		public void ArrayNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitArrayHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
using System.Linq;

using Neo.Tests.Framework;

using Neo.Frontend.Lexer;
using Neo.AST.Expressions.Atoms;

namespace Neo.Tests.AST.Expressions.Atoms {
	[TestFixture]
	public sealed class ObjectNodeTests {
		private ObjectNode subject;

		public ObjectNodeTests() {
			subject = new ObjectNode(new SourcePosition(2, 4));
		}
		
		[Test]
		public void ObjectNodesHaveASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(2, 4));
		}

		[Test]
		public void ObjectNodesHaveDefaultElements() {
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "foo"), new IntNode(SourcePosition.NIL, 42));
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "bar"), new IntNode(SourcePosition.NIL, 21));

			var defaultElements = subject.DefaultElements.ToList();
			Assert.AreEqual(defaultElements.Count, 2);
			Assert.AreEqual(((StringNode) defaultElements[0].Item1).Value, "foo");
			Assert.AreEqual(((StringNode) defaultElements[1].Item1).Value, "bar");
			Assert.AreEqual(((IntNode) defaultElements[0].Item2).Value, 42);
			Assert.AreEqual(((IntNode) defaultElements[1].Item2).Value, 21);
		}

		[Test]
		public void ObjectNodesAreNotConstantIfTheyContainANonConstantElement() {
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "foo"), new IntNode(SourcePosition.NIL, 42));
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "bar"), new IdentNode(SourcePosition.NIL, "foo"));
	
			Assert.AreEqual(subject.IsConstant(), false);			
		}

		[Test]
		public void ArrayNodesAreConstantIfTheyAreEmpty() {
			Assert.AreEqual(subject.IsConstant(), true);	
		}

		[Test]
		public void ObjectNodesAreConstantIfTheyOnlyContainConstantElements() {
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "foo"), new IntNode(SourcePosition.NIL, 42));
			subject.AddDefaultElement(new StringNode(SourcePosition.NIL, "bar"), new IntNode(SourcePosition.NIL, 21));
	
			Assert.AreEqual(subject.IsConstant(), true);	
		}

		[Test]
		public void ObjectNodesCanBeVisited() {
			object got = null;
			var testVisitor = new TestASTVisitor();
			testVisitor.VisitObjectHandler = node => got = node;

			subject.Accept(testVisitor);

			Assert.AreEqual(got, subject);
		}
	}
}
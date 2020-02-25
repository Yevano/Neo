using Neo.Tests.Framework;

using Neo.Frontend.Lexer;

namespace Neo.Tests.Frontend.Lexer {
	[TestFixture]
	public sealed class SourcePositionTests {
		private SourcePosition subject;

		public SourcePositionTests() {
			subject = new SourcePosition(42, 18);
		}

		[Test]
		public void HasALineAndNumber() {
			Assert.AreEqual(subject.Line, 42);
		}

		[Test]
		public void HasAColumnAndNumber() {
			Assert.AreEqual(subject.Column, 18);
		}

		[Test]
		public void CanBeStringFormatted() {
			Assert.AreEqual(subject.ToString(), "42:18");
		}
	}
}
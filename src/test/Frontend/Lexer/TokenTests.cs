using Neo.Tests.Framework;

using Neo.Frontend.Lexer;

namespace Neo.Tests.Frontend.Lexer {
	[TestFixture]
	public sealed class TokenTests {
		private Token subject;

		public TokenTests() {
			subject = new Token(TokenType.ADD_ASSIGN, "+=", new SourcePosition(13, 8));
		}
		
		[Test]
		public void HasATokenType() {
			Assert.AreEqual(subject.Type, TokenType.ADD_ASSIGN);
		}

		[Test]
		public void HasText() {
			Assert.AreEqual(subject.Text, "+=");
		}

		[Test]
		public void HasASourcePosition() {
			Assert.AreEqual(subject.Position, new SourcePosition(13, 8));
		}

		[Test]
		public void CanBeStringFormatted() {
			Assert.AreEqual(subject.ToString(), "ADD_ASSIGN(+=)@13:8");
		}
	}
}
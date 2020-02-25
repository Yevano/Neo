using Neo.Tests.Framework;

using Neo.Frontend.Lexer;

namespace Neo.Tests.Frontend.Lexer {
	[TestFixture]
	public sealed class LexExceptionTests {
		[Test]
		public void LexExceptionHasAMessage() {
			Assert.Throws<LexException>(() => {
				throw new LexException("test message");
			}, e => {
				Assert.That(e.Message == "test message");
			});
		}
	}
}
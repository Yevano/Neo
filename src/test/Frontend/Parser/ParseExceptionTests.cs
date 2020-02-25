using Neo.Tests.Framework;

using Neo.Frontend.Parser;

namespace Neo.Tests.Frontend.Parser {
	[TestFixture]
	public sealed class ParseExceptionTests {
		[Test]
		public void ParseExceptionHasAMessage() {
			Assert.Throws<ParseException>(() => {
				throw new ParseException("test message");
			}, e => {
				Assert.That(e.Message == "test message");
			});
		}
	}
}
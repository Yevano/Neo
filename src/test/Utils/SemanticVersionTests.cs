using System.Collections.Generic;
using System;

using Neo.Tests.Framework;

using Neo.Utils;

namespace Neo.Tests.Utils {
	[TestFixture]
	public sealed class SemanticVersionTests {
		[Test]
		public void SemanticVersionsHaveAMajorMinorAndPatchByte() {
			var subject = new SemanticVersion(4, 2, 0);

			Assert.AreEqual(subject.Major, (byte) 4);
			Assert.AreEqual(subject.Minor, (byte) 2);
			Assert.AreEqual(subject.Patch, (byte) 0);
		}

		[Test]
		public void SemanticVersionComparisonWorks() {
			var a = new SemanticVersion(0, 1, 2);
			var b = new SemanticVersion(0, 1, 3);
			var c = new SemanticVersion(0, 2, 1);
			var d = new SemanticVersion(1, 3, 0);
			var e = new SemanticVersion(1, 3, 1);

			var list = new List<SemanticVersion>();
			list.Add(b);
			list.Add(e);
			list.Add(d);
			list.Add(c);
			list.Add(a);

			list.Sort();

			Assert.AreEqual(list[0], a);
			Assert.AreEqual(list[1], b);
			Assert.AreEqual(list[2], c);
			Assert.AreEqual(list[3], d);
			Assert.AreEqual(list[4], e);
		}
		
		[Test]
		public void SemanticVersionEqualsOperatorWorks() {
			Assert.That(new SemanticVersion(0, 1, 2) == new SemanticVersion(0, 1, 2));

			Assert.That(!(new SemanticVersion(0, 1, 2) == new SemanticVersion(0, 1, 3)));
			Assert.That(!(new SemanticVersion(0, 1, 2) == new SemanticVersion(0, 2, 2)));
			Assert.That(!(new SemanticVersion(0, 1, 2) == new SemanticVersion(1, 1, 2)));
		}

		[Test]
		public void SemanticVersionNotEqualsOperatorWorks() {
			Assert.That(new SemanticVersion(0, 1, 2) != new SemanticVersion(0, 1, 3));
			Assert.That(new SemanticVersion(0, 1, 2) != new SemanticVersion(0, 2, 2));
			Assert.That(new SemanticVersion(0, 1, 2) != new SemanticVersion(1, 1, 2));

			Assert.That(!(new SemanticVersion(0, 1, 2) != new SemanticVersion(0, 1, 2)));
		}

		[Test]
		public void SemanticVersionGreaterThanWorks() {
			Assert.That(new SemanticVersion(0, 1, 3) > new SemanticVersion(0, 1, 2));
			Assert.That(new SemanticVersion(0, 2, 2) > new SemanticVersion(0, 1, 2));
			Assert.That(new SemanticVersion(1, 1, 2) > new SemanticVersion(0, 1, 2));
		
			Assert.That(!(new SemanticVersion(0, 1, 2) > new SemanticVersion(0, 1, 2)));

			Assert.That(!(new SemanticVersion(0, 1, 1) > new SemanticVersion(0, 1, 2)));
			Assert.That(!(new SemanticVersion(0, 0, 2) > new SemanticVersion(0, 1, 2)));
			Assert.That(!(new SemanticVersion(0, 1, 2) > new SemanticVersion(1, 1, 2)));
		}

		[Test]
		public void SemanticVersionLessThanWorks() {
			Assert.That(new SemanticVersion(0, 1, 2) < new SemanticVersion(0, 1, 3));
			Assert.That(new SemanticVersion(0, 1, 2) < new SemanticVersion(0, 2, 2));
			Assert.That(new SemanticVersion(0, 1, 2) < new SemanticVersion(1, 1, 2));
		
			Assert.That(!(new SemanticVersion(0, 1, 2) < new SemanticVersion(0, 1, 2)));

			Assert.That(!(new SemanticVersion(0, 1, 2) < new SemanticVersion(0, 1, 1)));
			Assert.That(!(new SemanticVersion(0, 1, 2) < new SemanticVersion(0, 0, 2)));
			Assert.That(!(new SemanticVersion(1, 1, 2) < new SemanticVersion(0, 1, 2)));
		}

		[Test]
		public void SemanticVersionGreaterThanOrEqualToWorks() {
			Assert.That(new SemanticVersion(0, 1, 3) >= new SemanticVersion(0, 1, 2));
			Assert.That(new SemanticVersion(0, 2, 2) >= new SemanticVersion(0, 1, 2));
			Assert.That(new SemanticVersion(1, 1, 2) >= new SemanticVersion(0, 1, 2));
		
			Assert.That(new SemanticVersion(0, 1, 2) >= new SemanticVersion(0, 1, 2));

			Assert.That(!(new SemanticVersion(0, 1, 1) >= new SemanticVersion(0, 1, 2)));
			Assert.That(!(new SemanticVersion(0, 0, 2) >= new SemanticVersion(0, 1, 2)));
			Assert.That(!(new SemanticVersion(0, 1, 2) >= new SemanticVersion(1, 1, 2)));
		}

		[Test]
		public void SemanticVersionLessThanOrEqualToWorks() {
			Assert.That(new SemanticVersion(0, 1, 2) <= new SemanticVersion(0, 1, 3));
			Assert.That(new SemanticVersion(0, 1, 2) <= new SemanticVersion(0, 2, 2));
			Assert.That(new SemanticVersion(0, 1, 2) <= new SemanticVersion(1, 1, 2));
		
			Assert.That(new SemanticVersion(0, 1, 2) <= new SemanticVersion(0, 1, 2));

			Assert.That(!(new SemanticVersion(0, 1, 2) <= new SemanticVersion(0, 1, 1)));
			Assert.That(!(new SemanticVersion(0, 1, 2) <= new SemanticVersion(0, 0, 2)));
			Assert.That(!(new SemanticVersion(1, 1, 2) <= new SemanticVersion(0, 1, 2)));
		}

		[Test]
		public void ToStringWorks() {
			Assert.AreEqual(new SemanticVersion(4, 2, 0).ToString(), "4.2.0");
		}

		[Test]
		public void ParseThrowsAnExceptionIfTheInputIsMalformed() {
			Assert.Throws<ArgumentException>(() => SemanticVersion.Parse("4.2.x"), e => Assert.AreEqual(e.Message, "Malformed semantic version: '4.2.x'"));
			Assert.Throws<ArgumentException>(() => SemanticVersion.Parse("4.x.0"), e => Assert.AreEqual(e.Message, "Malformed semantic version: '4.x.0'"));
			Assert.Throws<ArgumentException>(() => SemanticVersion.Parse("x.2.0"), e => Assert.AreEqual(e.Message, "Malformed semantic version: 'x.2.0'"));

			Assert.Throws<ArgumentException>(() => SemanticVersion.Parse("4.2"), e => Assert.AreEqual(e.Message, "Malformed semantic version: '4.2'"));
		}

		[Test]
		public void ParseWorks() {
			Assert.AreEqual(SemanticVersion.Parse("4.2.0"), new SemanticVersion(4, 2, 0));
		}

		[Test]
		public void TryParseThrowsAnExceptionIfTheInputDoesNotHaveEnoughPeriods() {
			SemanticVersion result;
			Assert.Throws<ArgumentException>(() => SemanticVersion.TryParse("4.2", out result), e => Assert.AreEqual(e.Message, "Malformed semantic version: '4.2'"));
		}

		[Test]
		public void TryParseSetsItsResultToTheDefaultIfTheInputIsMalformed() {
			var result = new SemanticVersion(9, 8, 7);

			Assert.AreEqual(SemanticVersion.TryParse("4.2.x", out result), false);
			Assert.AreEqual(result, new SemanticVersion(0, 0, 0));

			result = new SemanticVersion(9, 8, 7);

			Assert.AreEqual(SemanticVersion.TryParse("4.x.0", out result), false);
			Assert.AreEqual(result, new SemanticVersion(0, 0, 0));

			result = new SemanticVersion(9, 8, 7);

			Assert.AreEqual(SemanticVersion.TryParse("x.2.0", out result), false);
			Assert.AreEqual(result, new SemanticVersion(0, 0, 0));
		}

		[Test]
		public void TryParseWorks() {
			SemanticVersion result;

			Assert.AreEqual(SemanticVersion.TryParse("4.2.0", out result), true);
			Assert.AreEqual(result, new SemanticVersion(4, 2, 0));
		}
	}
}
using Neo.Tests.Framework;

using Neo.Utils;

namespace Neo.Tests.Utils {
	[TestFixture]
	public sealed class ArraysTests {
		[Test]
		public void ConcatWorksWithASingleEmptyArray() {
			var result = Arrays.Concat(new int[0]);

			Assert.AreEqual(result.Length, 0);
		}

		[Test]
		public void ConcatWorksWithMultipleEmptyArrays() {
			var result = Arrays.Concat(
				new int[0],
				new int[0],
				new int[0]
			);

			Assert.AreEqual(result.Length, 0);
		}

		[Test]
		public void ConcatWorksWithASingleNonEmptyArray() {
			var result = Arrays.Concat(new int[] { 42 });

			Assert.AreEqual(result.Length, 1);
			Assert.AreEqual(result[0], 42);
		}

		[Test]
		public void ConcatWorksWithMultipleNonEmptyArrays() {
			var result = Arrays.Concat(
				new int[] { 42 },
				new int[] { 1, 3 },
				new int[] { 21 }
			);

			Assert.AreEqual(result.Length, 4);
			Assert.AreEqual(result[0], 42);
			Assert.AreEqual(result[1], 1);
			Assert.AreEqual(result[2], 3);
			Assert.AreEqual(result[3], 21);
		}

		[Test]
		public void ConcatWorksWithASingleNonEmptyArrayAndASingleEmptyArray() {
			var result = Arrays.Concat(
				new int[0],
				new int[] { 42 }
			);

			Assert.AreEqual(result.Length, 1);
			Assert.AreEqual(result[0], 42);
		}

		[Test]
		public void ConcatWorksWithMultipleNonEmptyArraysAndMultipleEmptyArrays() {
			var result = Arrays.Concat(
				new int[0],
				new int[] { 42 },
				new int[0],
				new int[] { 1, 3 },
				new int[] { 21 },
				new int[0],
				new int[0]
			);

			Assert.AreEqual(result.Length, 4);
			Assert.AreEqual(result[0], 42);
			Assert.AreEqual(result[1], 1);
			Assert.AreEqual(result[2], 3);
			Assert.AreEqual(result[3], 21);
		}
	}
}
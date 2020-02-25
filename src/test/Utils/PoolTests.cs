using System;

using Neo.Tests.Framework;

using Neo.Utils;

namespace Neo.Tests.Utils {
	[TestFixture]
	public sealed class PoolTests {
		private Pool<string> subject;

		public PoolTests() {
			subject = new Pool<string>();
		}

		[Test]
		public void AddElementThrowsWhenADuplicateElementIsAdded() {
			Assert.Throws<InvalidOperationException>(() => {
				subject.Add("green");
				subject.Add("green");
			});			
		}

		[Test]
		public void AddsElementWithAMonotonicallyIncrementingIndex() {
			Assert.AreEqual(subject.Add("apple"), 0);
			Assert.AreEqual(subject.Add("orange"), 1);
			Assert.AreEqual(subject.Add("blue"), 2);
		}

		[Test]
		public void ContainsWorks() {
			Assert.AreEqual(subject.Contains("foo"), false);
			Assert.AreEqual(subject.Contains("bar"), false);

			subject.Add("foo");

			Assert.AreEqual(subject.Contains("foo"), true);
			Assert.AreEqual(subject.Contains("bar"), false);
		}

		[Test]
		public void IndexOfReturnsTheCorrectIndexWhenTheElementExistsInThePool() {
			var a = subject.Add("apple");
			var b = subject.Add("orange");
			var c = subject.Add("green");

			Assert.AreEqual(subject.IndexOf("apple"), a);
			Assert.AreEqual(subject.IndexOf("orange"), b);
			Assert.AreEqual(subject.IndexOf("green"), c);
		}

		[Test]
		public void IndexOfThrowsAnExceptionWhenTheElementDoesNotExistInThePool() {
			Assert.Throws<InvalidOperationException>(() => {
				subject.IndexOf("green");
			});
		}

		[Test]
		public void ToArrayWorks() {
			Assert.AreEqual(subject.ToArray().Length, 0);

			subject.Add("apple");
			subject.Add("green");

			var result = subject.ToArray();
			Assert.That(Array.IndexOf(result, "apple") > -1);
			Assert.That(Array.IndexOf(result, "green") > -1);
		}

		[Test]
		public void IndexOperatorWithElementThatIsNotInThePoolAddsTheElement() {
			Assert.AreEqual(subject["test"], 0);
			Assert.AreEqual(subject["fish"], 1);
			Assert.AreEqual(subject["let me out"], 2);
		} 

		[Test]
		public void IndexOperatorWithElementThatIsInThePoolAddsTheElement() {
			var a = subject["b*tch lazagna"];
			var b = subject["i'm sorry i didn't clarify"];

			Assert.AreEqual(subject["b*tch lazagna"], a);
			Assert.AreEqual(subject["i'm sorry i didn't clarify"], b);
		}

		[Test]
		public void IndexOperatorWithIndexThatIsNotInThePoolThrowsAnException() {
			Assert.Throws<InvalidOperationException>(() => {
				var x = subject[4];
			});			
		}

		[Test]
		public void IndexOperatorWithIndexThatIsInThePoolReturnsTheCorrectIndex() {
			var a = subject["let me out"];
			var b = subject["this is not a dance"];

			Assert.AreEqual(subject[a], "let me out");
			Assert.AreEqual(subject[b], "this is not a dance");
		}
		
		[Test]
		public void CountWorks() {
			Assert.AreEqual(subject.Count, 0);

			subject.Add("apple");

			Assert.AreEqual(subject.Count, 1);

			subject.Add("green");

			Assert.AreEqual(subject.Count, 2);
		}
	}
}
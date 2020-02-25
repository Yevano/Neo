using System;

namespace Neo.Tests.Framework {
	public sealed class AssertFailedException : Exception {
		public AssertFailedException(string msg = "") : base(msg) {}
	}

	public static class Assert {
		public static void That(bool x, string msg = "") {
			if(!x) {
				if(msg == "") {
					throw new AssertFailedException();
				} else {
					throw new AssertFailedException(msg);
				}
			}
		}

		public static void AreEqual(object actual, object expected, string msg = "") {
			if(msg == "") {
				Assert.That(expected.Equals(actual), $"Expected '{expected}', got '{actual}'");
			} else {
				Assert.That(expected.Equals(actual), msg);
			}
		}

		public static void Throws<T>(Action test, Action<T> handler = null, string msg = "") where T : Exception {
			try {
				test();
			} catch(T e) {
				if(handler != null)	handler(e);
				return;
			}

			if(msg == "") {
				throw new AssertFailedException("Exception expected but not thrown");
			} else {
				throw new AssertFailedException(msg);
			}
		}
	}
}
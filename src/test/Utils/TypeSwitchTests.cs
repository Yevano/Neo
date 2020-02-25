using System;

using Neo.Tests.Framework;

using Neo.Utils;

namespace Neo.Tests.Utils {
	[TestFixture]
	public sealed class TypeSwitchTests {
		[Test]
		public void CaseCreatesCaseInfoObjectCorrectly() {
			Action action = () => {};

			var result = TypeSwitch.Case<int>(action);

			Assert.AreEqual(result.IsDefault, false);
			Assert.AreEqual(result.Target, typeof(int));
			Assert.AreEqual(result.Action, action);
		}

		[Test]
		public void DefaultCreatesCaseInfoObjectCorrectly() {
			Action action = () => {};

			var result = TypeSwitch.Default(action);

			Assert.AreEqual(result.IsDefault, true);
			Assert.Throws<NullReferenceException>(() => Assert.AreEqual(result.Target, null));
			Assert.AreEqual(result.Action, action);
		}

		[Test]
		public void DoSelectsCorrectCase() {
			TypeSwitch.Do<int>(
				TypeSwitch.Case<string>(() => Assert.That(false, "Expected int, got string")),
				TypeSwitch.Case<bool>(() => Assert.That(false, "Expected int, got bool")),
				TypeSwitch.Case<int>(() => {}),
				TypeSwitch.Case<float>(() => Assert.That(false, "Expected int, got float"))
			);
		}

		[Test]
		public void DoDoesNothingIfTypesDoNotMatchAndNoDefaultCaseExists() {
			TypeSwitch.Do<object>(
				TypeSwitch.Case<string>(() => Assert.That(false, "Expected default case, got string")),
				TypeSwitch.Case<bool>(() => Assert.That(false, "Expected default case, got bool")),
				TypeSwitch.Case<int>(() => Assert.That(false, "Expected default case, got int")),
				TypeSwitch.Case<float>(() => Assert.That(false, "Expected default case, got float"))
			);
		}

		[Test]
		public void DoSelectsDefaultCaseIfTypesDoNotMatch() {
			var result = false;

			TypeSwitch.Do<object>(
				TypeSwitch.Case<string>(() => Assert.That(false, "Expected default case, got string")),
				TypeSwitch.Case<bool>(() => Assert.That(false, "Expected default case, got bool")),
				TypeSwitch.Case<int>(() => Assert.That(false, "Expected default case, got int")),
				TypeSwitch.Case<float>(() => Assert.That(false, "Expected default case, got float")),
				TypeSwitch.Default(() => result = true)
			);
			
			Assert.That(result, "Default case was not invoked!");
		}
	}
}
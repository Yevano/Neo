using System;

namespace Neo.Tests.Framework {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TestFixture : Attribute {
		public TestFixture(bool enabled = true) {
			Enabled = enabled;
		}
		
		public bool Enabled { get; }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class Test : Attribute {
		public Test(bool enabled = true) {
			Enabled = enabled;
		}
		
		public bool Enabled { get; }
	}
}
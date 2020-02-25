using System;

using Neo.Bytecode;

namespace Neo.Runtime.Native {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class NativeModule : Attribute {
		public NativeModule(string name, bool hasNeoSource = false) {
			Name = name;
			HasNeoSource = hasNeoSource;
		}

		public string Name { get; }

		public bool HasNeoSource { get; }
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class NativeValue : Attribute {
		public NativeValue(string name) {
			Name = name;
		}

		public string Name { get; }
	}
}
using Neo.Bytecode;
using Neo.Runtime;

namespace Neo.Runtime.Native {
	public sealed class NativeValueInfo {
		public NativeValueInfo(string name, NeoValue value) {
			Name = name;
			Value = value;
		}

		public string Name { get; }

		public NeoValue Value { get; }
	}
}
using System.Collections.Generic;

using Neo.Runtime;

namespace Neo.Runtime.Native {
	public sealed class NativeModuleInfo {
		public NativeModuleInfo(string name, bool hasNeoSource) {
			Name = name;
			HasNeoSource = hasNeoSource;
			Values = new List<NativeValueInfo>();
		}
		
		public string Name { get; }

		public bool HasNeoSource { get; }

		public List<NativeValueInfo> Values { get; }
	}
}
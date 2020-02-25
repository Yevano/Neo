using System.IO;

namespace Neo.Runtime.Native.Modules {
	[NativeModule("std/io/file", true)]
	public static class FileLib {
		[NativeValue("getParent")]
		public static NeoValue GetParent(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("getParent expects a single string argument"); //@TODO @Untested
			var path = args[0].CheckString().Value;
			return NeoString.ValueOf(Directory.GetParent(Path.GetFullPath(path)).FullName);
		}

		[NativeValue("list")]
		public static NeoValue List(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("list expects a single string argument"); //@TODO @Untested
			var path = args[0].CheckString().Value;
			var result = new NeoArray();
			foreach(var file in Directory.GetFiles(Path.GetFullPath(path))) {
				result.Insert(NeoString.ValueOf(file));
			}
			return result;
		}

		[NativeValue("getName")]
		public static NeoValue GetName(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("getName expects a single string argument"); //@TODO @Untested
			var path = args[0].CheckString().Value;
			return NeoString.ValueOf(Path.GetFileName(path));
		}

		[NativeValue("exists")]
		public static NeoValue Exists(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("exists expects a single string argument"); //@TODO @Untested
			var path = args[0].CheckString().Value;
			return NeoBool.ValueOf(File.Exists(Path.GetFullPath(path)));
		}

		[NativeValue("readAllText")]
		public static NeoValue ReadAllText(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("readAllText expects a single string argument"); //@TODO @Untested
			var path = args[0].CheckString().Value;
			return NeoString.ValueOf(File.ReadAllText(Path.GetFullPath(path)));			
		}

		[NativeValue("writeAllText")]
		public static NeoValue WriteAllText(NeoValue[] args) {
			if(args.Length != 2) throw new NeoError("readAllText expects two string arguments"); //@TODO @Untested
			var path = args[0].CheckString().Value;
			var text = args[1].CheckString().Value;
			File.WriteAllText(path, text);
			return NeoNil.NIL;
		}
	}	
}
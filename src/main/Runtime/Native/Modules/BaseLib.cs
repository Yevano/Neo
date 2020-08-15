using System;
using System.Threading;

namespace Neo.Runtime.Native.Modules {
	[NativeModule("std/base", true)]
	public static class BaseLib {
		[NativeValue("write")]
		public static NeoValue Write(NeoValue[] args) {
			foreach(var arg in args) Console.Write(arg.ToNeoString());
			return NeoNil.NIL;
		}

		[NativeValue("print")]
		public static NeoValue Print(NeoValue[] args) {
			if(args.Length == 0) {
				Console.WriteLine();
			} else {
				foreach(var arg in args) Console.WriteLine(arg.ToNeoString());
			}
			return NeoNil.NIL;
		}

		[NativeValue("exit")]
		public static NeoValue Exit(NeoValue[] args) {
			if(args.Length != 0 && args.Length != 1) throw new NeoError("exit expects either zero arguments or one int argument"); // @TODO @Untested
			
			if(args.Length == 1) {
				Environment.Exit(args[0].CheckInt().Value);
			} else {
				Environment.Exit(0);
			}

			return NeoNil.NIL; 
		}

		[NativeValue("getMetaObject")]
		public static NeoValue GetMetaObject(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("getMetaObject expects a single object argument"); // @TODO @Untested
			var obj = args[0].CheckObject();
			if(obj.MetaObject != null) return obj.MetaObject;
			else return NeoNil.NIL;
		}

		[NativeValue("setMetaObject")]
		public static NeoValue SetMetaObject(NeoValue[] args) {
			if(args.Length != 2) throw new NeoError("setMetaObject expects two object arguments"); // @TODO @Untested
			var obj = args[0].CheckObject();
			var mo = args[1].CheckObject();
			obj.MetaObject = mo;
			return obj;
		}

		[NativeValue("rawGet")]
		public static NeoValue RawGet(NeoValue[] args) {
			if(args.Length != 2) throw new NeoError("rawGet expects an object argument and an index argument"); // @TODO @Untested
			var obj = args[0].CheckObject();
			return obj.RawGet(args[1]);
		}

		[NativeValue("rawSet")]
		public static NeoValue RawSet(NeoValue[] args) {
			if(args.Length != 2) throw new NeoError("rawSet expects an object argument, an index argument, and a value argument"); // @TODO @Untested
			var obj = args[0].CheckObject();
			obj.RawSet(args[1], args[2]);
			return NeoNil.NIL;
		}

		[NativeValue("type")]
		public static NeoValue Type(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("exit expects one argument"); // @TODO @Untested
			return NeoString.ValueOf(args[0].Type);
		}

		[NativeValue("read")]
		public static NeoValue Read(NeoValue[] args) {
			return NeoInt.ValueOf(Console.Read());
		}

		[NativeValue("sleep")]
		public static NeoValue Sleep(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("sleep expects a single int argument"); // @TODO @Untested
			Thread.Sleep(args[0].CheckInt().Value);
			return NeoNil.NIL;
		}

		[NativeValue("toString")]
		public static NeoValue ToString(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("toString expects a single argument"); // @TODO @Untested
			return NeoString.ValueOf(args[0].ToNeoString());
		}

		[NativeValue("int")]
		public static NeoValue Int(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("int expects a single numeric argument"); // @TODO @Untested
			return NeoInt.ValueOf(args[0].CheckNumber().CastToInt());
		}

		[NativeValue("float")]
		public static NeoValue Float(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("float expects a single numeric argument"); // @TODO @Untested
			return NeoFloat.ValueOf(args[0].CheckNumber().CastToFloat());
		}

		[NativeValue("parseNumber")]
		public static NeoValue ParseNumber(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("float expects a single string argument"); // @TODO @Untested
			return NeoNumber.Reify(double.Parse(args[0].CheckString().Value));
		}

		[NativeValue("insert")]
		public static NeoValue Insert(NeoValue[] args) {
			if(args.Length != 2) throw new NeoError("insert expects an array and a value as arguments"); // @TODO @Untested
			args[0].CheckArray().Insert(args[1]);
			return NeoNil.NIL;
		}

		[NativeValue("remove")]
		public static NeoValue Remove(NeoValue[] args) {
			if(args.Length != 2) throw new NeoError("remove expects an array and an index as arguments"); // @TODO @Untested
			args[0].CheckArray().Remove(args[1]);
			return NeoNil.NIL;
		}
	} 
}
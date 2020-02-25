using System;

namespace Neo.Runtime.Native.Modules {
	[NativeModule("std/math", true)]
	public static class MathLib {
		[NativeValue("sin")]
		public static NeoValue Sin(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("sin expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Sin(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("cos")]
		public static NeoValue Cos(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("cos expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Cos(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("tan")]
		public static NeoValue Tan(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("tan expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Tan(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("asin")]
		public static NeoValue Asin(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("asin expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Asin(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("acos")]
		public static NeoValue Acos(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("acos expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Acos(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("atan")]
		public static NeoValue Atan(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("atan expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Atan(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("floor")]
		public static NeoValue Floor(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("floor expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Floor(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("ceil")]
		public static NeoValue Ceil(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("ceil expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Ceiling(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("round")]
		public static NeoValue Round(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("round expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Round(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("sqrt")]
		public static NeoValue Sqrt(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("sqrt expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Sqrt(args[0].CheckNumber().AsDouble));
		}

		[NativeValue("exp")]
		public static NeoValue Exp(NeoValue[] args) {
			if(args.Length != 1) throw new NeoError("exp expects a single numeric argument"); // @TODO @Untested
			return NeoNumber.Reify(Math.Exp(args[0].CheckNumber().AsDouble));
		}
	}
}
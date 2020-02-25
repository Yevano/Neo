namespace Neo.Runtime.Native.Modules {
	[NativeModule("std/string", true)]
    public static class StringLib {
    	[NativeValue("byte")]
        public static NeoValue ToByte(NeoValue[] args) {
        	if(args.Length != 1) throw new NeoError("byte expects a single string parameter"); // @TODO @Untested
        	var str = args[0].CheckString().Value;
        	if(str.Length != 1) throw new NeoError($"byte expects its parameter to be of length 1, got {str.Length}"); // @TODO @Untested
        	return NeoInt.ValueOf(str[0]);
        }

        [NativeValue("char")]
        public static NeoValue ToChar(NeoValue[] args) {
        	if(args.Length != 1) throw new NeoError("char expects a single int parameter"); // @TODO @Untested
        	var val = args[0].CheckInt().Value;
        	return NeoString.ValueOf(char.ToString((char) val));
        }
    	
    	[NativeValue("upper")]
    	public static NeoValue ToUpper(NeoValue[] args) {
    		if(args.Length != 1) throw new NeoError("upper expects a single string parameter"); // @TODO @Untested
    		return NeoString.ValueOf(args[0].CheckString().Value.ToUpper());
    	}

    	[NativeValue("lower")]
    	public static NeoValue ToLower(NeoValue[] args) {
    		if(args.Length != 1) throw new NeoError("lower expects a single string parameter"); // @TODO @Untested
    		return NeoString.ValueOf(args[0].CheckString().Value.ToLower());
    	}
    }
}
using System;

namespace Neo.Runtime {
    public sealed class NeoError : Exception {
    	public NeoError(string message) : this(message, -1) {
    	}

        public NeoError(string message, int line) : base(message) {
        	Line = line;
        }

        public int Line { get; set; }
    }
}
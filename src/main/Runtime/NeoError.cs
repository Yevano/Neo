using System;

namespace Neo.Runtime {
    public sealed class NeoError : Exception {
    	public NeoError(string message) : this(message, -1) {
    	}

        public NeoError(string message, int line) : base(message) {
        	Line = line;
        }

        public NeoError(string message, string chunkName, int line) : base(message) {
        	ChunkName = chunkName;
            Line = line;
        }

        public string ChunkName { get; set;}

        public int Line { get; set; }
    }
}
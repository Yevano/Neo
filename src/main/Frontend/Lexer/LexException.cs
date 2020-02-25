using System;

namespace Neo.Frontend.Lexer {
    [Serializable]
    public sealed class LexException : Exception {
        public LexException(string message) : base(message) {
        }
    }
}
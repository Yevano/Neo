using System;

namespace Neo.Frontend.Parser {
    [Serializable]
    public sealed class ParseException : Exception {
        public ParseException(string message) : base(message) {
        }
    }
}

namespace Neo.Frontend.Lexer {
    public struct Token {
        public Token(TokenType type, string text, SourcePosition position) {
            Type = type;
            Text = text;
            Position = position;
        }

        public TokenType Type { get; }

        public string Text { get; }

        public SourcePosition Position { get; }

        public override string ToString() {
            return $"{Type}({Text})@{Position}";
        }
    }
}
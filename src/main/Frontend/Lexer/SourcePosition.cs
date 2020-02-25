namespace Neo.Frontend.Lexer {
	public struct SourcePosition {
		public static readonly SourcePosition NIL = new SourcePosition(0, 0);
		
		public readonly int Line;
		public readonly int Column;

		public SourcePosition(int line, int column) {
			Line = line;
			Column = column;
		}

		public override string ToString() {
            return $"{Line}:{Column}";
        }
	}
}
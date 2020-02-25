namespace Neo.Bytecode {
	public struct LineRange {
		public LineRange(int line, int start, int end) {
			Line = line;
			Start = start;
			End = end;
		}

		public int Line { get; }

		public int Start { get; }

		public int End { get; }

		public override string ToString() => $"{Line}: {Start} - {End}";
	}
}
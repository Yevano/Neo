namespace Neo.Bytecode {
	public struct Import {
		public Import(string path, string alias) {
			Path = path;
			Alias = alias;
		}

		public string Path { get; }

		public string Alias { get; }
	}
}
using Neo.AST;
using Neo.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neo.Bytecode {
    public sealed class Chunk {
        internal Chunk(BinaryReader reader) {
            var magic = new[] { (byte)0x1B, (byte)0x4E, (byte)0x65, (byte)0x6F };
            for (var i = 0; i < 4; i++) {
                if (reader.ReadByte() != magic[i]) throw new Exception("invalid chunk");
            }

            var chunkVersion = new SemanticVersion(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            if (chunkVersion != VM.VERSION) {
                throw new Exception("chunk out of date");
            }

            Name = reader.ReadString();

            Imports = new List<Import>();
            var imports = reader.ReadInt32();
            for(var i = 0; i < imports; i++) {
            	var path = reader.ReadString();
            	var alias = reader.ReadString();
            	Imports.Add(new Import(path, alias));
            }

            Initializer = new Procedure(reader);

            Variables = new Dictionary<string, Variable>();
            var vars = reader.ReadInt32();
            for (var i = 0; i < vars; i++) {
                var name = reader.ReadString();
                var flags = reader.ReadByte();
                Variables[name] = new Variable(name, (VariableFlags)flags);
            }

            Enums = new Dictionary<string, EnumDeclaration>();
            var enums = reader.ReadInt32();
            for (var i = 0; i < enums; i++) {
                var name = reader.ReadString();
                var elements = reader.ReadInt32();
                var exported = reader.ReadBoolean();

                var e = new List<string>();
                for (var j = 0; j < elements; j++) e.Add(reader.ReadString());

                Enums[name] = new EnumDeclaration(name, e, exported);
            }

            Procedures = new Dictionary<string, Procedure>();
            var procs = reader.ReadInt32();
            for (var i = 0; i < procs; i++) {
                var proc = new Procedure(reader);
                Procedures[proc.Name] = proc;
            }
        }

        public Chunk(ChunkNode ast) {
            Name = ast.Name;
            Procedures = new Dictionary<string, Procedure>();
            Variables = new Dictionary<string, Variable>();
            Enums = new Dictionary<string, EnumDeclaration>();
            Imports = new List<Import>();
        }

        public string Name { get; }

        public Procedure Initializer { get; set; }

        public Dictionary<string, Procedure> Procedures { get; }

        public Dictionary<string, Variable> Variables { get; }

        public Dictionary<string, EnumDeclaration> Enums { get; }

        public List<Import> Imports { get; }

        public byte[] Serialize() {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            writer.Write((byte)0x1B);
            writer.Write((byte)0x4E);
            writer.Write((byte)0x65);
            writer.Write((byte)0x6F);
            writer.Write(VM.VERSION.Major);
            writer.Write(VM.VERSION.Minor);
            writer.Write(VM.VERSION.Patch);

            writer.Write(Name);

            writer.Write(Imports.Count);
            foreach(var import in Imports) {
            	writer.Write(import.Path);
            	writer.Write(import.Alias);
            }

            writer.Write(Initializer.Serialize());

            writer.Write(Variables.Count);
            foreach (var v in Variables) {
                writer.Write(v.Value.Name);
                writer.Write((byte)v.Value.Flags);
            }

            writer.Write(Enums.Count);
            foreach (var e in Enums) {
                writer.Write(e.Key);
                writer.Write(e.Value.Elements.Count);
                foreach (var el in e.Value.Elements) writer.Write(el);
            }

            writer.Write(Procedures.Count);
            foreach (var p in Procedures) {
                writer.Write(p.Value.Serialize());
            }

            return ms.ToArray();
        }
    }
}
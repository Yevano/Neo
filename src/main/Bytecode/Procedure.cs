using Neo.Runtime;
using System;
using System.IO;

namespace Neo.Bytecode {
    public sealed class Procedure {
        internal Procedure(BinaryReader reader) {
            Name = reader.ReadString();
            Exported = reader.ReadBoolean();

            UpValues = new string[reader.ReadInt32()];
            for (var i = 0; i < UpValues.Length; i++) {
                UpValues[i] = reader.ReadString();
            }

            Parameters = new Parameter[reader.ReadInt32()];
            for (var i = 0; i < Parameters.Length; i++) {
                Parameters[i] = new Parameter(reader.ReadString(), reader.ReadBoolean());
            }

            Varargs = reader.ReadBoolean();

            Lines = new LineRange[reader.ReadInt32()];
            for(var i = 0; i < Lines.Length; i++) {
                Lines[i] = new LineRange(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            }

            Line = reader.ReadInt32();

            Instructions = new byte[reader.ReadInt32()];
            for (var i = 0; i < Instructions.Length; i++) {
                Instructions[i] = reader.ReadByte();
            }

            Constants = new NeoValue[reader.ReadInt32()];
            for (var i = 0; i < Constants.Length; i++) {
                var type = reader.ReadByte();
                switch (type) {
                    case (byte)ConstantType.STRING:
                        Constants[i] = NeoString.ValueOf(reader.ReadString());
                        break;
                    case (byte)ConstantType.INT:
                        Constants[i] = NeoInt.ValueOf(reader.ReadInt32());
                        break;
                    case (byte)ConstantType.FLOAT:
                        Constants[i] = NeoFloat.ValueOf(reader.ReadDouble());
                        break;
                    default:
                        throw new Exception("invalid chunk");
                }
            }

            Procedures = new Procedure[reader.ReadInt32()];
            for (var i = 0; i < Procedures.Length; i++) {
                Procedures[i] = new Procedure(reader);
            }
        }

        public Procedure(string name, bool exported, string[] upvalues, Parameter[] parameters, bool varargs, LineRange[] lines, int line, byte[] instructions, NeoValue[] constants, Procedure[] procedures) {
            Name = name;
            Exported = exported;
            UpValues = upvalues;
            Parameters = parameters;
            Varargs = varargs;
            Lines = lines;
            Line = line;
            Instructions = instructions;
            Constants = constants;
            Procedures = procedures;
        }

        public string Name { get; }

        public bool Exported { get; }

        public string[] UpValues { get; }

        public Parameter[] Parameters { get; }

        public bool Varargs { get; }

        public LineRange[] Lines { get; }

        public int Line { get; }

        public byte[] Instructions { get; }

        public NeoValue[] Constants { get; }

        public Procedure[] Procedures { get; }

        public int FindLine(int x) {
            foreach(var range in Lines) {
                if(x >= range.Start && x <= range.End) {
                    return range.Line;
                }
            }
            return -1;
        }

        public byte[] Serialize() {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            writer.Write(Name);
            writer.Write(Exported);

            writer.Write(UpValues.Length);
            foreach (var upvalue in UpValues) {
                writer.Write(upvalue);
            }

            writer.Write(Parameters.Length);
            foreach (var parameter in Parameters) {
                writer.Write(parameter.Name);
                writer.Write(parameter.Frozen);
            }

            writer.Write(Varargs);

            writer.Write(Lines.Length);
            foreach(var line in Lines) {
                writer.Write(line.Line);
                writer.Write(line.Start);
                writer.Write(line.End);
            }

            writer.Write(Line);

            writer.Write(Instructions.Length);
            writer.Write(Instructions);

            writer.Write(Constants.Length);
            foreach (var constant in Constants) {
                if (constant.IsString) {
                    writer.Write((byte)ConstantType.STRING);
                    writer.Write(constant.CheckString().Value);
                } else if (constant.IsInt) {
                    writer.Write((byte)ConstantType.INT);
                    writer.Write(constant.CheckInt().Value);
                } else if (constant.IsFloat) {
                    writer.Write((byte)ConstantType.FLOAT);
                    writer.Write(constant.CheckFloat().Value);
                } else {
                    throw new Exception(constant.ToString());
                }
            }

            writer.Write(Procedures.Length);
            foreach (var p in Procedures) {
                writer.Write(p.Serialize());
            }

            return ms.ToArray();
        }
    }
}
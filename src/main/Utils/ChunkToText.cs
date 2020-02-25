using Neo.Bytecode;
using Neo.Runtime;
using System.Text;

namespace Neo.Utils {
    public static class ChunkToText {
        public static string Encode(Chunk chunk) {
            var sb = new StringBuilder();

            void Append(object o) {
                if (o is bool) {
                    sb.Append(o.ToString().ToLower());
                } else {
                    sb.Append(o.ToString());
                }
            }

            void AppendLine(object o = null) {
                if (o == null) {
                    sb.AppendLine();
                } else if (o is bool) {
                    sb.AppendLine(o.ToString().ToLower());
                } else {
                    sb.AppendLine(o.ToString());
                }
            }

            var indent = 0;
            void Indent() => Append(new string(' ', indent));
            void Increase() => indent += 4;
            void Decrease() => indent -= 4;

            void AppendInstructions(byte[] instructions, NeoValue[] constants) {
                var ip = 4;

                OpCode ReadOpCode() => (OpCode)instructions[ip++];
                byte ReadByte() => instructions[ip++];
                short ReadShort() => (short)(instructions[ip++] | (instructions[ip++] << 8));
                int ReadInt() => instructions[ip++] | (instructions[ip++] << 8) | (instructions[ip++] << 16) | (instructions[ip++] << 24);
                NeoValue ReadConstant() => constants[ReadInt()];

                Pool<int> labels = new Pool<int>();
                while (ip < instructions.Length) {
                    var op = ReadOpCode();
                    switch (op) {
                        case OpCode.JUMP:
                        case OpCode.JUMP_IF: {
                                var addr = ReadInt();
                                if (!labels.Contains(addr)) {
                                    labels.Add(addr);
                                }
                            }
                            break;
                    }
                }

                ip = 4;

                while (ip < instructions.Length) {
                    if (labels.Contains(ip)) {
                        Indent();
                        Append(".label l");
                        AppendLine(labels.Count - labels.IndexOf(ip) - 1);
                    }

                    var opip = ip;
                    var op = ReadOpCode();

                    Indent();
                    Append(op.ToString().ToLower());
                    switch (op) {
                        case OpCode.JUMP:
                        case OpCode.JUMP_IF: {
                                Append(" l");
                                Append(labels.Count - labels.IndexOf(ReadInt()) - 1);
                            }
                            break;
                        case OpCode.CALL: {
                                Append(" ");
                                Append(ReadShort());
                            }
                            break;
                        case OpCode.RETURN: {
                                Append(" ");
                                Append(ReadByte() == 1 ? "true" : "false");
                            }
                            break;
                        case OpCode.PUSH_CONSTANT:
                        case OpCode.GET_LOCAL:
                        case OpCode.SET_LOCAL:
                        case OpCode.GET_GLOBAL:
                        case OpCode.SET_GLOBAL:
                        case OpCode.GET_UPVALUE:
                        case OpCode.SET_UPVALUE:
                        case OpCode.CLOSURE:
                        case OpCode.CLOSE: {
                                Append(" k(");
                                Append(ReadConstant());
                                Append(")");
                            }
                            break;
                        case OpCode.DECLARE: {
                                Append(" k(");
                                Append(ReadConstant());
                                Append(") ");
                                Append((VariableFlags)ReadByte());
                            }
                            break;
                    }
                    Append(" # ");
                    Append(opip);
                    AppendLine();
                }
            }

            void AppendProcedure(Procedure p) {
                Indent();
                Append(".proc ");
                Append(p.Name);
                Append(" ");
                Append(p.Exported);
                Append(" ");
                Append(p.Varargs);
                AppendLine();
                Increase();
                foreach(var upvalue in p.UpValues) {
                    Indent();
                    Append(".upvalue ");
                    Append(upvalue);
                    AppendLine("\n");
                }
                foreach (var param in p.Parameters) {
                    Indent();
                    Append(".param ");
                    Append(param.Name);
                    Append(" ");
                    Append(param.Frozen);
                    AppendLine("\n");
                }
                foreach (var proc in p.Procedures) {
                    AppendProcedure(proc);
                    AppendLine();
                }
                AppendInstructions(p.Instructions, p.Constants);
                Decrease();
                Indent();
                AppendLine(".end");
            }

            Append(".chunk ");
            AppendLine(chunk.Name);

            AppendLine();

            if (chunk.Variables.Count > 0) {
                foreach (var v in chunk.Variables.Values) {
                    Append(".var ");
                    Append(v.Name);
                    Append(" ");
                    Append(v.Flags);
                    AppendLine("\n");
                }
            }

            foreach (var proc in chunk.Procedures.Values) {
                AppendProcedure(proc);
                AppendLine();
            }

            return sb.ToString().Trim();
        }
    }
}
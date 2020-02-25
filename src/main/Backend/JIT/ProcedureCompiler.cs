using Neo.Bytecode;
using Neo.Runtime;
using Neo.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Neo.Backend.JIT {
    internal sealed class ProcedureCompiler {
        private readonly VM vm;
        private readonly Scope parentScope;
        private readonly Chunk chunk;
        private readonly Procedure procedure;
        private readonly UpValue[] upvalues;

        public ProcedureCompiler(VM vm, Scope parentScope, Chunk chunk, Procedure procedure, UpValue[] upvalues) {
            this.vm = vm;
            this.parentScope = parentScope;
            this.chunk = chunk;
            this.procedure = procedure;
            this.upvalues = upvalues;
        }

        public IProcedureImplementation Compile() {
            var dm = new DynamicMethod(procedure.Name, typeof(NeoValue), new[] { typeof(Scope), typeof(NeoValue[]), typeof(NeoValue), typeof(Dictionary<string, UpValue>), typeof(VM), typeof(Scope), typeof(Chunk), typeof(Procedure) }, typeof(VM).Module);
            var il = dm.GetILGenerator();

            var scope = il.DeclareLocal(typeof(Scope));

            var nvTemp1 = il.DeclareLocal(typeof(NeoValue));
            var nvTemp2 = il.DeclareLocal(typeof(NeoValue));
            var nvaTemp1 = il.DeclareLocal(typeof(NeoValue[]));
            var intTemp1 = il.DeclareLocal(typeof(int));

            var openUps = il.DeclareLocal(typeof(Dictionary<string, UpValue>));
            var upvalues = il.DeclareLocal(typeof(UpValue[]));
            var lproc = il.DeclareLocal(typeof(Procedure));

            var defers = il.DeclareLocal(typeof(Stack<NeoProcedure>));

            var lineForError = il.DeclareLocal(typeof(int));

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stloc, scope);

            il.Emit(OpCodes.Newobj, typeof(Dictionary<string, UpValue>).GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc, openUps);

            il.Emit(OpCodes.Newobj, typeof(Stack<NeoProcedure>).GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc, defers);

            for (var i = 0; i < procedure.Parameters.Length; i++) {
                il.Emit(OpCodes.Ldloc, scope);
                il.Emit(OpCodes.Ldstr, procedure.Parameters[i].Name);

                var label = il.DefineLabel();
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, typeof(NeoValue[]).GetMethod("get_Length"));
                il.Emit(OpCodes.Blt, label);
                il.Emit(OpCodes.Ldsfld, typeof(NeoNil).GetField("NIL"));
                var end = il.DefineLabel();
                il.Emit(OpCodes.Br, end);
                il.MarkLabel(label);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem, typeof(NeoValue));
                il.MarkLabel(end);

                if(procedure.Parameters[i].Frozen) {
                    il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Frozen"));
                }

                il.Emit(OpCodes.Call, typeof(Scope).GetMethod("Set", new[] { typeof(string), typeof(NeoValue) }));
            }

            var code = procedure.Instructions;
            var ip = 4;

            byte ReadByte() => code[ip++];
            Bytecode.OpCode ReadOpCode() => (Bytecode.OpCode)ReadByte();
            short ReadShort() => (short)(ReadByte() | (ReadByte() << 8));
            int ReadInt() => ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24);
            NeoValue ReadConstant() => procedure.Constants[ReadInt()];

            var labels = new Dictionary<int, System.Reflection.Emit.Label>();
            while (ip < code.Length) {
                var op = ReadOpCode();
                switch (op) {
                    case Bytecode.OpCode.JUMP:
                    case Bytecode.OpCode.JUMP_IF: {
                            labels[ReadInt()] = il.DefineLabel();
                        }
                        break;
                }
            }

            ip = 4;

            var exBlock = il.BeginExceptionBlock();

            var rangeIndex = 0;
            var currentRange = procedure.Lines[rangeIndex];

            void UpdateLineLocal() {
                il.Emit(OpCodes.Ldc_I4, currentRange.Line);
                il.Emit(OpCodes.Stloc, lineForError);
            }

            UpdateLineLocal();

            while (ip < code.Length) {
                if (labels.ContainsKey(ip)) {
                    il.MarkLabel(labels[ip]);
                }

                if(ip > currentRange.End) {
                    rangeIndex++;
                    currentRange = procedure.Lines[rangeIndex];
                    UpdateLineLocal();
                }

                var op = ReadOpCode();
                switch (op) {
                    case Bytecode.OpCode.NOP:
                        break;
                    case Bytecode.OpCode.ADD: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Add", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.INC: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Inc"));
                        }
                        break;
                    case Bytecode.OpCode.DEC: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Dec"));
                        }
                        break;
                    case Bytecode.OpCode.SUB: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Sub", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.MUL: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Mul", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.DIV: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Div", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.POW: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Pow", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.MOD: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Mod", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.LSH: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Lsh", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.RSH: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Rsh", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.BIT_NOT: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("BitNot", new Type[0]));
                        }
                        break;
                    case Bytecode.OpCode.BIT_AND: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("BitAnd", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.BIT_OR: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("BitOr", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.BIT_XOR: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("BitXor", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.NOT: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Not", new Type[0]));
                        }
                        break;
                    case Bytecode.OpCode.NEG: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Neg", new Type[0]));
                        }
                        break;
                    case Bytecode.OpCode.CONCAT: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Concat", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.LENGTH: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Length", new Type[0]));
                        }
                        break;
                    case Bytecode.OpCode.ARRAY_NEW: {
                            il.Emit(OpCodes.Newobj, typeof(NeoArray).GetConstructor(new Type[0]));
                        }
                        break;
                    case Bytecode.OpCode.ARRAY_ADD: {
                            il.Emit(OpCodes.Stloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckArray", new Type[0]));
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(NeoArray).GetMethod("Insert", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.OBJECT_NEW: {
                            il.Emit(OpCodes.Newobj, typeof(NeoObject).GetConstructor(new Type[0]));
                        }
                        break;
                    case Bytecode.OpCode.OBJECT_INDEX: {
                            il.Emit(OpCodes.Stloc, nvTemp1);
                            il.Emit(OpCodes.Stloc, nvTemp2);

                            il.Emit(OpCodes.Ldloc, nvTemp2);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckObject"));
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckInt"));
                            il.Emit(OpCodes.Call, typeof(NeoInt).GetMethod("get_Value"));
                            il.Emit(OpCodes.Call, typeof(NeoObject).GetMethod("Index", new[] { typeof(int) }));

                            il.Emit(OpCodes.Ldloc, nvTemp2);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckObject"));
                            il.Emit(OpCodes.Ldloc, nvTemp2);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckObject"));
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckInt"));
                            il.Emit(OpCodes.Call, typeof(NeoInt).GetMethod("get_Value"));
                            il.Emit(OpCodes.Call, typeof(NeoObject).GetMethod("Index", new[] { typeof(int) }));
                            il.Emit(OpCodes.Call, typeof(NeoObject).GetMethod("Get"));
                        }
                        break;
                    case Bytecode.OpCode.GET: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Get", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.SET: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Set", new[] { typeof(NeoValue), typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.SLICE: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Slice", new[] { typeof(NeoValue), typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.EQ: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Eq", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.NE: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Ne", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.DEEP_EQ: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("DeepEq", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.DEEP_NE: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("DeepNq", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.LT: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Lt", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.GT: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Gt", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.LTE: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Lte", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.GTE: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Gte", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.CMP: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Compare", new[] { typeof(NeoValue) }));
                            il.Emit(OpCodes.Call, typeof(NeoInt).GetMethod("ValueOf", new [] { typeof(int) }));
                        }
                        break;
                    case Bytecode.OpCode.JUMP: {
                            il.Emit(OpCodes.Br, labels[ReadInt()]);
                        }
                        break;
                    case Bytecode.OpCode.JUMP_IF: {
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckBool"));
                            il.Emit(OpCodes.Call, typeof(NeoBool).GetMethod("get_Value"));
                            il.Emit(OpCodes.Brtrue, labels[ReadInt()]);
                        }
                        break;
                    case Bytecode.OpCode.CALL: {
                            var nargs = ReadShort();

                            il.Emit(OpCodes.Ldc_I4, (int)nargs);
                            il.Emit(OpCodes.Newarr, typeof(NeoValue));
                            il.Emit(OpCodes.Stloc, nvaTemp1);

                            for (var i = nargs - 1; i >= 0; i--) {
                                il.Emit(OpCodes.Stloc, nvTemp1);
                                il.Emit(OpCodes.Ldloc, nvaTemp1);
                                il.Emit(OpCodes.Ldc_I4, i);
                                il.Emit(OpCodes.Ldloc, nvTemp1);
                                il.Emit(OpCodes.Stelem, typeof(NeoValue));
                            }

                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Stloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("get_IsProcedure"));
                            var notProc = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse, notProc);

                            il.Emit(OpCodes.Ldarg, 4);
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Callvirt, typeof(NeoProcedure).GetMethod("ChunkName"));
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Callvirt, typeof(NeoProcedure).GetMethod("Name"));
                            il.Emit(OpCodes.Ldloc, lineForError);

                            var commonEnd = il.DefineLabel();
                            il.Emit(OpCodes.Br, commonEnd);
                            
                            il.MarkLabel(notProc);

                            il.Emit(OpCodes.Ldarg, 4);
                            il.Emit(OpCodes.Ldstr, "<unknown>");
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldloc, lineForError);

                            il.MarkLabel(commonEnd);

                            il.Emit(OpCodes.Call, typeof(VM).GetMethod("PushFrame", BindingFlags.Instance | BindingFlags.NonPublic, null, new [] { typeof(string), typeof(string), typeof(int) }, null));

                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Ldloc, nvaTemp1);
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Call", new[] { typeof(NeoValue[]) }));

                            il.Emit(OpCodes.Ldarg, 4);
                            il.Emit(OpCodes.Call, typeof(VM).GetMethod("PopFrame", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null));
                        }
                        break;
                    case Bytecode.OpCode.RETURN: {
                            il.Emit(OpCodes.Ldloc, openUps);
                            il.Emit(OpCodes.Call, typeof(JITProcedure).GetMethod("CloseAll", new[] { typeof(Dictionary<string, UpValue>) }));

                            il.Emit(OpCodes.Ldloc, defers);
                            il.Emit(OpCodes.Call, typeof(JITProcedure).GetMethod("RunDefers", new[] { typeof(Stack<NeoProcedure>) }));

                            if (ReadByte() == 0) {
                                il.Emit(OpCodes.Ldsfld, typeof(NeoNil).GetField("NIL"));
                            }
                            il.Emit(OpCodes.Ret);
                        }
                        break;
                    case Bytecode.OpCode.DEFER: {
                            il.Emit(OpCodes.Stloc, nvTemp1);
                            il.Emit(OpCodes.Ldloc, defers);
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckProcedure", new Type[0]));
                            il.Emit(OpCodes.Call, typeof(Stack<NeoProcedure>).GetMethod("Push", new[] { typeof(NeoProcedure) }));
                        }
                        break;
                    case Bytecode.OpCode.VARARGS: {
                            il.Emit(OpCodes.Ldarg_2);
                        }
                        break;
                    case Bytecode.OpCode.SPREAD: {
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckArray"));
                            il.Emit(OpCodes.Newobj, typeof(NeoSpreadValue).GetConstructor(new[] { typeof(NeoArray) }));
                        }
                        break;
                    case Bytecode.OpCode.FROZEN: {
                            il.Emit(OpCodes.Callvirt, typeof(NeoValue).GetMethod("Frozen"));
                        }
                        break;
                    case Bytecode.OpCode.PUSH_TRUE: {
                            il.Emit(OpCodes.Ldsfld, typeof(NeoBool).GetField("TRUE"));
                        }
                        break;
                    case Bytecode.OpCode.PUSH_FALSE: {
                            il.Emit(OpCodes.Ldsfld, typeof(NeoBool).GetField("FALSE"));
                        }
                        break;
                    case Bytecode.OpCode.PUSH_NIL: {
                            il.Emit(OpCodes.Ldsfld, typeof(NeoNil).GetField("NIL"));
                        }
                        break;
                    case Bytecode.OpCode.PUSH_CONSTANT: {
                            var k = ReadConstant();
                            if (k is NeoInt i) {
                                il.Emit(OpCodes.Ldc_I4, i.Value);
                                il.Emit(OpCodes.Call, typeof(NeoInt).GetMethod("ValueOf", new[] { typeof(int) }));
                            } else if (k is NeoFloat f) {
                                il.Emit(OpCodes.Ldc_R8, f.Value);
                                il.Emit(OpCodes.Call, typeof(NeoFloat).GetMethod("ValueOf", new[] { typeof(double) }));
                            } else if (k is NeoString s) {
                                il.Emit(OpCodes.Ldstr, s.Value);
                                il.Emit(OpCodes.Call, typeof(NeoString).GetMethod("ValueOf", new[] { typeof(string) }));
                            } else {
                                throw new Exception();
                            }
                        }
                        break;
                    case Bytecode.OpCode.DUP: {
                            il.Emit(OpCodes.Dup);
                        }
                        break;
                    case Bytecode.OpCode.SWAP: {
                            il.Emit(OpCodes.Stloc, nvTemp1);
                            il.Emit(OpCodes.Stloc, nvTemp2);
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Ldloc, nvTemp2);
                        }
                        break;
                    case Bytecode.OpCode.POP: {
                            il.Emit(OpCodes.Pop);
                        }
                        break;
                    case Bytecode.OpCode.CLOSURE: {
                            var name = ReadConstant().CheckString().Value;
                            var proc = procedure.Procedures.First(p => p.Name == name);

                            il.Emit(OpCodes.Ldc_I4, proc.UpValues.Length);
                            il.Emit(OpCodes.Newarr, typeof(UpValue));
                            il.Emit(OpCodes.Stloc, upvalues);

                            il.Emit(OpCodes.Ldc_I4_0);
                            il.Emit(OpCodes.Stloc, intTemp1);
                            var start = il.DefineLabel();
                            il.MarkLabel(start);
                            il.Emit(OpCodes.Ldarg, 7);
                            il.Emit(OpCodes.Call, typeof(Procedure).GetMethod("get_Procedures"));
                            il.Emit(OpCodes.Ldloc, intTemp1);
                            il.Emit(OpCodes.Ldelem, typeof(Procedure));
                            il.Emit(OpCodes.Call, typeof(Procedure).GetMethod("get_Name"));
                            il.Emit(OpCodes.Ldstr, name);
                            il.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality"));
                            var codeEnd = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse, codeEnd);
                            il.Emit(OpCodes.Ldarg, 7);
                            il.Emit(OpCodes.Call, typeof(Procedure).GetMethod("get_Procedures"));
                            il.Emit(OpCodes.Ldloc, intTemp1);
                            il.Emit(OpCodes.Ldelem, typeof(Procedure));
                            il.Emit(OpCodes.Stloc, lproc);
                            var end = il.DefineLabel();
                            il.Emit(OpCodes.Br, end);
                            il.MarkLabel(codeEnd);
                            il.Emit(OpCodes.Ldloc, intTemp1);
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Add);
                            il.Emit(OpCodes.Stloc, intTemp1);
                            il.Emit(OpCodes.Br, start);
                            il.MarkLabel(end);

                            for (var i = 0; i < proc.UpValues.Length; i++) {
                                var pop = ReadOpCode();
                                if (pop != Bytecode.OpCode.GET_LOCAL && pop != Bytecode.OpCode.GET_UPVALUE) {
                                    throw new Exception(pop.ToString());
                                }

                                var upname = ReadConstant().CheckString().Value;

                                switch (pop) {
                                    case Bytecode.OpCode.GET_LOCAL: {
                                            il.Emit(OpCodes.Ldloc, openUps);
                                            il.Emit(OpCodes.Ldstr, upname);
                                            il.Emit(OpCodes.Call, typeof(Dictionary<string, UpValue>).GetMethod("ContainsKey", new[] { typeof(string) }));
                                            var label = il.DefineLabel();
                                            il.Emit(OpCodes.Brtrue, label);
                                            il.Emit(OpCodes.Ldloc, openUps);
                                            il.Emit(OpCodes.Ldstr, upname);
                                            il.Emit(OpCodes.Ldloc, scope);
                                            il.Emit(OpCodes.Ldstr, upname);
                                            il.Emit(OpCodes.Newobj, typeof(UpValue).GetConstructor(new[] { typeof(Scope), typeof(string) }));
                                            il.Emit(OpCodes.Call, typeof(Dictionary<string, UpValue>).GetMethod("set_Item", new[] { typeof(string), typeof(UpValue) }));
                                            il.MarkLabel(label);

                                            il.Emit(OpCodes.Ldloc, upvalues);
                                            il.Emit(OpCodes.Ldc_I4, i);
                                            il.Emit(OpCodes.Ldloc, openUps);
                                            il.Emit(OpCodes.Ldstr, upname);
                                            il.Emit(OpCodes.Call, typeof(Dictionary<string, UpValue>).GetMethod("get_Item"));
                                            il.Emit(OpCodes.Stelem, typeof(UpValue));
                                        }
                                        break;
                                    case Bytecode.OpCode.GET_UPVALUE: {
                                            il.Emit(OpCodes.Ldloc, upvalues);
                                            il.Emit(OpCodes.Ldc_I4, i);
                                            il.Emit(OpCodes.Ldarg_3);
                                            il.Emit(OpCodes.Ldstr, upname);
                                            il.Emit(OpCodes.Call, typeof(Dictionary<string, UpValue>).GetMethod("get_Item", new[] { typeof(string) }));
                                            il.Emit(OpCodes.Stelem, typeof(UpValue));
                                        }
                                        break;
                                }
                            }

                            il.Emit(OpCodes.Ldarg, 4);
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Call, typeof(VM).GetMethod("get_Interpreter"));
                            il.Emit(OpCodes.Ldarg, 0);
                            il.Emit(OpCodes.Ldarg, 6);
                            il.Emit(OpCodes.Ldloc, lproc);
                            il.Emit(OpCodes.Ldloc, upvalues);
                            il.Emit(OpCodes.Callvirt, typeof(IBackend).GetMethod("Compile", new[] { typeof(Scope), typeof(Chunk), typeof(Procedure), typeof(UpValue[]) }));
                            il.Emit(OpCodes.Newobj, typeof(NeoBackendProcedure).GetConstructor(new[] { typeof(VM), typeof(IProcedureImplementation) }));
                        }
                        break;
                    case Bytecode.OpCode.CLOSE: {
                            var name = ReadConstant().CheckString().Value;

                            il.Emit(OpCodes.Ldloc, openUps);
                            il.Emit(OpCodes.Ldstr, name);
                            il.Emit(OpCodes.Call, typeof(JITProcedure).GetMethod("Close", new[] { typeof(Dictionary<string, UpValue>), typeof(string) }));
                        }
                        break;
                    case Bytecode.OpCode.GET_LOCAL: {
                            il.Emit(OpCodes.Ldloc, scope);
                            il.Emit(OpCodes.Ldstr, ReadConstant().CheckString().Value);
                            il.Emit(OpCodes.Call, typeof(Scope).GetMethod("Get", new[] { typeof(string) }));
                        }
                        break;
                    case Bytecode.OpCode.SET_LOCAL: {
                            il.Emit(OpCodes.Stloc, nvTemp1);
                            il.Emit(OpCodes.Ldloc, scope);
                            il.Emit(OpCodes.Ldstr, ReadConstant().CheckString().Value);
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(Scope).GetMethod("Set", new[] { typeof(string), typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.GET_GLOBAL: {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldstr, ReadConstant().CheckString().Value);
                            il.Emit(OpCodes.Call, typeof(Scope).GetMethod("Get", new[] { typeof(string) }));
                        }
                        break;
                    case Bytecode.OpCode.SET_GLOBAL: {
                            il.Emit(OpCodes.Stloc, nvTemp1);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldstr, ReadConstant().CheckString().Value);
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(Scope).GetMethod("Set", new[] { typeof(string), typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.GET_UPVALUE: {
                            il.Emit(OpCodes.Ldarg_3);
                            il.Emit(OpCodes.Ldstr, ReadConstant().CheckString().Value);
                            il.Emit(OpCodes.Call, typeof(Dictionary<string, NeoValue>).GetMethod("get_Item", new[] { typeof(string) }));
                            il.Emit(OpCodes.Call, typeof(UpValue).GetMethod("Get"));
                        }
                        break;
                    case Bytecode.OpCode.SET_UPVALUE: {
                            il.Emit(OpCodes.Stloc, nvTemp1);
                            il.Emit(OpCodes.Ldarg_3);
                            il.Emit(OpCodes.Ldstr, ReadConstant().CheckString().Value);
                            il.Emit(OpCodes.Call, typeof(Dictionary<string, NeoValue>).GetMethod("get_Item", new[] { typeof(string) }));
                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(UpValue).GetMethod("Set", new[] { typeof(NeoValue) }));
                        }
                        break;
                    case Bytecode.OpCode.TRY: {
                            il.Emit(OpCodes.Stloc, nvTemp2);
                            il.Emit(OpCodes.Stloc, nvTemp1);

                            il.Emit(OpCodes.Ldloc, nvTemp1);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckProcedure"));
                            il.Emit(OpCodes.Ldloc, nvTemp2);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckProcedure"));
                            il.Emit(OpCodes.Call, typeof(JITProcedure).GetMethod("TryCatch", new[] { typeof(NeoProcedure), typeof(NeoProcedure) }));

                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("get_IsNil"));
                            var label = il.DefineLabel();
                            il.Emit(OpCodes.Brtrue, label);
                            il.Emit(OpCodes.Ldloc, openUps);
                            il.Emit(OpCodes.Call, typeof(JITProcedure).GetMethod("CloseAll", new[] { typeof(Dictionary<string, UpValue>) }));
                            il.Emit(OpCodes.Ldloc, defers);
                            il.Emit(OpCodes.Call, typeof(JITProcedure).GetMethod("RunDefers", new[] { typeof(Stack<NeoProcedure>) }));
                            il.Emit(OpCodes.Ret);
                            il.MarkLabel(label);
                            il.Emit(OpCodes.Pop);
                        }
                        break;
                    case Bytecode.OpCode.THROW: {
                            il.Emit(OpCodes.Call, typeof(NeoValue).GetMethod("CheckString"));
                            il.Emit(OpCodes.Call, typeof(NeoString).GetMethod("get_Value"));
                            il.Emit(OpCodes.Ldloc, lineForError);
                            il.Emit(OpCodes.Newobj, typeof(NeoError).GetConstructor(new[] { typeof(string), typeof(int) }));
                            il.Emit(OpCodes.Throw);
                        }
                        break;
                    case Bytecode.OpCode.DECLARE: {
                            il.Emit(OpCodes.Ldloc, scope);
                            il.Emit(OpCodes.Ldstr, ReadConstant().CheckString().Value);
                            il.Emit(OpCodes.Ldc_I4, (int)ReadByte());
                            il.Emit(OpCodes.Call, typeof(Scope).GetMethod("Declare", new[] { typeof(string), typeof(VariableFlags) }));
                        }
                        break;
                    default: {
                            throw new Exception($"Unexpected opcode: {op}");
                        }
                }
            }

            var methodEnd = il.DefineLabel();
            il.Emit(OpCodes.Br, methodEnd);

            il.BeginCatchBlock(typeof(NeoError));

            il.Emit(OpCodes.Dup);

            var skip = il.DefineLabel();
            var eq = il.DefineLabel();
            il.Emit(OpCodes.Call, typeof(NeoError).GetMethod("get_Line"));
            il.Emit(OpCodes.Ldc_I4, -1);
            il.Emit(OpCodes.Beq, eq);
            il.Emit(OpCodes.Br, skip);
            il.MarkLabel(eq);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldloc, lineForError);
            il.Emit(OpCodes.Call, typeof(NeoError).GetMethod("set_Line"));
            il.MarkLabel(skip);

            il.Emit(OpCodes.Throw);

            il.EndExceptionBlock();

            il.MarkLabel(methodEnd);
            il.Emit(OpCodes.Ldsfld, typeof(NeoNil).GetField("NIL"));
            il.Emit(OpCodes.Ret);

            return new JITProcedure(vm, parentScope, chunk, procedure, this.upvalues, (ProcedureCall)dm.CreateDelegate(typeof(ProcedureCall)));
        }
    }

    internal delegate NeoValue ProcedureCall(Scope scope, NeoValue[] args, NeoValue varargs, Dictionary<string, UpValue> upvalues, VM vm, Scope parentScope, Chunk chunk, Procedure procedure);
}
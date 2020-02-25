using Neo.Bytecode;
using Neo.Runtime;
using Neo.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Backend.Interp {
    internal sealed class InterpProcedure : IProcedureImplementation {
        private readonly UpValue[] upvalues;

        public InterpProcedure(VM vm, Scope scope, Chunk chunk, Procedure procedure, UpValue[] upvalues) {
            VM = vm;
            ParentScope = scope;
            Chunk = chunk;
            Procedure = procedure;
            this.upvalues = upvalues;
        }

        public VM VM { get; }

        public Chunk Chunk { get; }

        public Procedure Procedure { get; }

        public Scope ParentScope { get; }

        public UpValue[] UpValues => upvalues;

        public NeoValue Call(NeoValue[] arguments) {
            var scope = new Scope(ParentScope);
            var level = scope.Level;

            var pargs = new List<NeoValue>();
            foreach (var arg in arguments) {
                if (arg is NeoSpreadValue va) {
                    for (var i = 0; i < va.Array.Count; i++) {
                        pargs.Add(va.Array[i]);
                    }
                } else {
                    pargs.Add(arg);
                }
            }

            var varargs = new NeoArray();
            var extraArgs = pargs.Count - Procedure.Parameters.Length;
            if (extraArgs > 0) {
                for (var i = 0; i < extraArgs; i++) {
                    varargs.Insert(pargs[Procedure.Parameters.Length + i]);
                }
            }

            for (var i = 0; i < Procedure.Parameters.Length; i++) {
                var name = Procedure.Parameters[i].Name;
                scope.Declare(name, VariableFlags.NONE);

                var value = i < pargs.Count ? pargs[i] : NeoNil.NIL;

                if(Procedure.Parameters[i].Frozen) {
                    value = value.Frozen();    
                }

                scope.Set(name, value);
            }

            var code = Procedure.Instructions;
            var ip = 4;
            var stack = new Stack<NeoValue>();

            var rootScopeLevel = scope.Level;

            var openUps = new Dictionary<string, UpValue>();

            var defers = new Stack<NeoProcedure>();

            byte ReadByte() => code[ip++];
            OpCode ReadOpCode() => (OpCode)ReadByte();
            short ReadShort() => (short)(ReadByte() | (ReadByte() << 8));
            int ReadInt() => ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24);
            NeoValue ReadConstant() => Procedure.Constants[ReadInt()];

            while (ip < code.Length) {
                var op = ReadOpCode();

                try {
                    switch (op) {
                        case OpCode.NOP:
                            break;
                        case OpCode.INC: {
                                stack.Push(stack.Pop().Inc());
                            }
                            break;
                        case OpCode.DEC: {
                                stack.Push(stack.Pop().Dec());
                            }
                            break;
                        case OpCode.ADD: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Add(b));
                            }
                            break;
                        case OpCode.SUB: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Sub(b));
                            }
                            break;
                        case OpCode.MUL: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Mul(b));
                            }
                            break;
                        case OpCode.DIV: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Div(b));
                            }
                            break;
                        case OpCode.POW: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Pow(b));
                            }
                            break;
                        case OpCode.MOD: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Mod(b));
                            }
                            break;
                        case OpCode.LSH: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Lsh(b));
                            }
                            break;
                        case OpCode.RSH: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Rsh(b));
                            }
                            break;
                        case OpCode.BIT_NOT: {
                                stack.Push(stack.Pop().BitNot());
                            }
                            break;
                        case OpCode.BIT_AND: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.BitAnd(b));
                            }
                            break;
                        case OpCode.BIT_OR: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.BitOr(b));
                            }
                            break;
                        case OpCode.BIT_XOR: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.BitXor(b));
                            }
                            break;
                        case OpCode.NOT: {
                                stack.Push(stack.Pop().Not());
                            }
                            break;
                        case OpCode.NEG: {
                                stack.Push(stack.Pop().Neg());
                            }
                            break;
                        case OpCode.CONCAT: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Concat(b));
                            }
                            break;
                        case OpCode.LENGTH: {
                                stack.Push(stack.Pop().Length());
                            }
                            break;
                        case OpCode.ARRAY_NEW: {
                                stack.Push(new NeoArray());
                            }
                            break;
                        case OpCode.ARRAY_ADD: {
                                var element = stack.Pop();
                                var array = stack.Pop().CheckArray();
                                array.Insert(element);
                            }
                            break;
                        case OpCode.OBJECT_NEW: {
                                stack.Push(new NeoObject());
                            }
                            break;
                        case OpCode.OBJECT_INDEX: {
                                var index = stack.Pop().CheckInt();
                                var obj = stack.Pop().CheckObject();
                                stack.Push(obj.Index(index.Value));
                                stack.Push(obj.Get(obj.Index(index.Value)));
                            }
                            break;
                        case OpCode.GET: {
                                var key = stack.Pop();
                                var obj = stack.Pop();
                                stack.Push(obj.Get(key));
                            }
                            break;
                        case OpCode.SET: {
                                var value = stack.Pop();
                                var key = stack.Pop();
                                var obj = stack.Pop();
                                obj.Set(key, value);
                            }
                            break;
                        case OpCode.SLICE: {
                                var end = stack.Pop();
                                var start = stack.Pop();
                                stack.Push(stack.Pop().Slice(start, end));
                            }
                            break;
                        case OpCode.EQ: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Eq(b));
                            }
                            break;
                        case OpCode.NE: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Ne(b));
                            }
                            break;
                        case OpCode.DEEP_EQ: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.DeepEq(b));
                            }
                            break;
                        case OpCode.DEEP_NE: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.DeepNe(b));
                            }
                            break;
                        case OpCode.LT: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Lt(b));
                            }
                            break;
                        case OpCode.GT: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Gt(b));
                            }
                            break;
                        case OpCode.LTE: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Lte(b));
                            }
                            break;
                        case OpCode.GTE: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a.Gte(b));
                            }
                            break;
                        case OpCode.CMP: {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(NeoInt.ValueOf(a.Compare(b)));
                            }
                            break;
                        case OpCode.JUMP: {
                                ip = ReadInt();
                            }
                            break;
                        case OpCode.JUMP_IF: {
                                var addr = ReadInt();
                                if (stack.Pop().CheckBool().Value) {
                                    ip = addr;
                                }
                            }
                            break;
                        case OpCode.CALL: {
                                var line = Procedure.FindLine(ip);
                                var nargs = ReadShort();

                                var args = new NeoValue[nargs];
                                for (var i = nargs - 1; i >= 0; i--) {
                                    args[i] = stack.Pop();
                                }

                                var callee = stack.Pop();
                                if(callee.IsProcedure) {
                                    var proc = callee.CheckProcedure();
                                    VM.PushFrame(proc.ChunkName(), proc.Name(), line);
                                    stack.Push(proc.Call(args));
                                    VM.PopFrame();
                                } else {
                                    VM.PushFrame("<unknown>", "<unknown>", line);
                                    stack.Push(callee.Call(args));
                                    VM.PopFrame();
                                }
                            }
                            break;
                        case OpCode.RETURN: {
                                foreach (var up in openUps.Values) {
                                    up.Close();
                                }

                                NeoValue rvalue = null;
                                if (ReadByte() == 1) {
                                    rvalue = stack.Pop();
                                } else {
                                    if (stack.Count > 0) throw new Exception("VM error: stack not empty");

                                    rvalue = NeoNil.NIL;
                                }

                                while (defers.Count > 0) {
                                    defers.Pop().Call(new NeoValue[0]);
                                }

                                return rvalue;
                            }
                        case OpCode.DEFER: {
                                defers.Push(stack.Pop().CheckProcedure());
                            }
                            break;
                        case OpCode.VARARGS: {
                                if (!Procedure.Varargs) {
                                    stack.Push(NeoNil.NIL);
                                } else {
                                    stack.Push(varargs);
                                }
                            }
                            break;
                        case OpCode.PUSH_TRUE: {
                                stack.Push(NeoBool.TRUE);
                            }
                            break;
                        case OpCode.PUSH_FALSE: {
                                stack.Push(NeoBool.FALSE);
                            }
                            break;
                        case OpCode.PUSH_NIL: {
                                stack.Push(NeoNil.NIL);
                            }
                            break;
                        case OpCode.PUSH_CONSTANT: {
                                stack.Push(ReadConstant());
                            }
                            break;
                        case OpCode.DUP: {
                                var a = stack.Pop();
                                stack.Push(a);
                                stack.Push(a);
                            }
                            break;
                        case OpCode.SWAP: {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                stack.Push(a);
                                stack.Push(b);
                            }
                            break;
                        case OpCode.POP: {
                                stack.Pop();
                            }
                            break;
                        case OpCode.CLOSURE: {
                                var name = ReadConstant().CheckString().Value;
                                var proc = Procedure.Procedures.First(p => p.Name == name);

                                var upvalues = new UpValue[proc.UpValues.Length];
                                for (var i = 0; i < upvalues.Length; i++) {
                                    var pop = ReadOpCode();
                                    if (pop != OpCode.GET_LOCAL && pop != OpCode.GET_UPVALUE) {
                                        throw new Exception(pop.ToString());
                                    }

                                    var upname = ReadConstant().CheckString().Value;

                                    switch (pop) {
                                        case OpCode.GET_LOCAL: {
                                                if (!openUps.ContainsKey(upname)) {
                                                    openUps[upname] = new UpValue(scope, upname);
                                                }
                                                upvalues[i] = openUps[upname];
                                            }
                                            break;
                                        case OpCode.GET_UPVALUE: {
                                                upvalues[i] = this.upvalues.First(up => up.Name == upname);
                                            }
                                            break;
                                    }
                                }

                                stack.Push(new NeoBackendProcedure(VM, VM.Interpreter.Compile(scope, Chunk, proc, upvalues)));
                            }
                            break;
                        case OpCode.CLOSE: {
                                var name = ReadConstant().CheckString().Value;
                                if (openUps.ContainsKey(name)) {
                                    openUps[name].Close();
                                    openUps.Remove(name);
                                }
                            }
                            break;
                        case OpCode.GET_LOCAL: {;
                                stack.Push(scope.Get(ReadConstant().CheckString().Value));
                            }
                            break;
                        case OpCode.SET_LOCAL: {
                                var name = ReadConstant().CheckString().Value;
                                var value = stack.Pop();
                                scope.Set(name, value);
                            }
                            break;
                        case OpCode.GET_GLOBAL: {
                                stack.Push(ParentScope.Get(ReadConstant().CheckString().Value));
                            }
                            break;
                        case OpCode.SET_GLOBAL: {
                                ParentScope.Set(ReadConstant().CheckString().Value, stack.Pop());
                            }
                            break;
                        case OpCode.GET_UPVALUE: {
                                var name = ReadConstant().CheckString().Value;
                                var up = upvalues.First(u => u.Name == name);
                                stack.Push(up.Get());
                            }
                            break;
                        case OpCode.SET_UPVALUE: {
                                var name = ReadConstant().CheckString().Value;

                                var index = 0;
                                for (var i = 0; i < upvalues.Length; i++) {
                                    if (upvalues[i].Name == name) {
                                        index = i;
                                        break;
                                    }
                                }

                                upvalues[index].Set(stack.Pop());
                            }
                            break;
                        case OpCode.SPREAD: {
                                stack.Push(new NeoSpreadValue(stack.Pop().CheckArray()));
                            }
                            break;
                        case OpCode.FROZEN: {
                                stack.Push(stack.Pop().Frozen());
                            }
                            break;
                        case OpCode.TRY: {
                                var @catch = stack.Pop().CheckProcedure();
                                var @try = stack.Pop().CheckProcedure();

                                NeoValue r;

                                try {
                                    r = @try.Call(new NeoValue[0]);
                                } catch (NeoError e) {
                                    r = @catch.Call(new[] { NeoString.ValueOf(e.Message) });
                                }

                                if(!r.IsNil) {
                                    foreach (var up in openUps.Values) {
                                        up.Close();
                                    }

                                    while (defers.Count > 0) {
                                        defers.Pop().Call(new NeoValue[0]);
                                    }

                                    return r;
                                }
                            }
                            break;
                        case OpCode.THROW: {
                                var line = Procedure.FindLine(ip);
                                throw new NeoError(stack.Pop().CheckString().Value, line);
                            }
                        case OpCode.DECLARE: {
                                var name = ReadConstant().CheckString().Value;
                                var flags = ReadByte();
                                scope.Declare(name, (VariableFlags)flags);
                            }
                            break;
                        default: {
                                throw new Exception($"Unexpected opcode: {op}");
                            }
                    }
                } catch(NeoError e) {
                    if(e.Line == -1) e.Line = Procedure.FindLine(ip);
                    throw e;
                }
            }

            throw new Exception("VM error");
        }
        
        public string Name() => Procedure.Name;

        public string ChunkName() => Chunk.Name;
    }
}
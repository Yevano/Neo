using Neo.AST;
using Neo.AST.Declarations;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;
using Neo.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Bytecode {
    public sealed class BytecodeCompiler : IASTVisitor {
        private readonly ChunkNode ast;

        private readonly Stream stream;
        private readonly BinaryWriter writer;

        private ProcedureWriter asm;
        private readonly Stack<ProcedureWriter> procedures;

        private Loop loop;
        private readonly Stack<Loop> loops;

        private HashSet<string> upValues;
        private Stack<HashSet<string>> upValuesStack;

        private Chunk chunk;

        public BytecodeCompiler(ChunkNode ast) {
            this.ast = ast;

            stream = new MemoryStream();
            writer = new BinaryWriter(stream);

            procedures = new Stack<ProcedureWriter>();
            loops = new Stack<Loop>();
            upValuesStack = new Stack<HashSet<string>>();

            chunk = new Chunk(ast);
        }

        private void NewProcedure(string name, bool exported, bool varargs, int line, bool upvalueOfSelf) {
            var current = asm;

            procedures.Push(asm);

            asm = new ProcedureWriter(name, exported, varargs, line);
            if (upvalueOfSelf) asm.MarkUpValue(name);

            if (current != null) {
                foreach (var param in current.Parameters) {
                    asm.MarkUpValue(param.Name);
                }

                foreach (var local in current.Locals) {
                    asm.MarkUpValue(local);
                }

                foreach (var upvalue in current.UpValues) {
                    asm.MarkUpValue(upvalue);
                }
            }
        }

        private void PushScope() {
            asm.PushScope();
        }

        private void PopScope() {
            asm.PopScope();
        }

        private void PushLoop(Label start, Label end, Label codeEnd) {
            loops.Push(loop);
            loop = new Loop() {
                Start = start,
                End = end,
                CodeEnd = codeEnd
            };
        }

        private void PopLoop() {
            loop = loops.Pop();
        }

        private void GetUpValue(string name) {
            if(upValues != null && !upValues.Contains(name)) {
                upValues.Add(name);
            }

            asm.GetUpValue(name);
        }

        private void SetUpValue(string name) {
            if(upValues != null && !upValues.Contains(name)) {
                upValues.Add(name);
            }

            asm.SetUpValue(name);
        }

        private void PushUpValueTracking() {
            upValuesStack.Push(upValues);
            upValues = new HashSet<string>();
        }

        private HashSet<string> PopUpValueTracking() {
            var result = upValues;
            upValues = upValuesStack.Pop();
            return result;
        }

        private void PopUpValueTrackingAndClose() {
            var ups = PopUpValueTracking();
            foreach(var up in ups) {
                asm.Close(up);
            }
        }

        private void Closure(Procedure proc) {
            asm.Closure(proc.Name);
            foreach (var upvalue in proc.UpValues) {
                if (asm.IsLocal(upvalue)) {
                    asm.GetLocal(upvalue);
                } else if (asm.IsUpValue(upvalue)) {
                    GetUpValue(upvalue);
                } else {
                    throw new Exception(upvalue);
                }
            }
        }

        private Procedure FinishProcedure(bool set = false) {
            var proc = asm.Finish();
            asm = procedures.Pop();

            if (asm != null) {
                asm.AddProcedure(proc);

                if(set) {
                    asm.MarkLocal(proc.Name);
                    asm.Declare(proc.Name, VariableFlags.FINAL);
                    Closure(proc);
                    asm.SetLocal(proc.Name);
                }
            } else {
                if (chunk.Procedures.ContainsKey(proc.Name)) throw new NeoError("attempt to redeclare '" + proc.Name + "'", ast.Name, proc.Line);
                chunk.Procedures[proc.Name] = proc;
            }

            return proc;
        }

        private void UpdateLine(Node node) {
            if(asm == null) return;
            asm.SetLine(node.Position.Line);
        }

        public Chunk Compile() {
            ast.Accept(this);
            return chunk;
        }

        public void VisitChunk(ChunkNode node) {
            foreach (var import in node.Imports) {
                import.Accept(this);
            }
            
            NewProcedure("__init", false, false, -1, false);
            UpdateLine(node);

            foreach (var v in node.Variables) v.Accept(this);
            foreach (var e in node.Enums) e.Accept(this);

            var init = node.Procedures.Where(p => p.Name.Value == "__init").FirstOrDefault();
            if (init != null) {
                if (init.Parameters.Count > 0) {
                    throw new NeoError("__init must take 0 parameters");
                }

                if (init.Varargs) {
                    throw new NeoError("__init must not be varargs");
                }

                if (init.Exported) {
                    throw new NeoError("__init must not be exported");
                }

                init.Statements.ForEach(s => s.Accept(this));
            }

            asm.Return();
            chunk.Initializer = FinishProcedure();

            foreach (var p in node.Procedures.Where(p => p.Name.Value != "__init")) {
                p.Accept(this);
            }
        }

        public void VisitProc(ProcNode node) {
            UpdateLine(node);
            NewProcedure(node.Name.Value, node.Exported, node.Varargs, node.Position.Line, true);
            UpdateLine(node);

            node.Parameters.ForEach(p => p.Accept(this));
            node.Statements.ForEach(s => s.Accept(this));

            if (node.Statements.Count == 0 || !(node.Statements.Last() is ReturnNode)) {
                // @TODO: this is not entirely correct; we need control flow analysis to do this properly.
                //                  - sci4me, Mar 31, 2019
                asm.Return();
            }

            FinishProcedure(true);
        }

        public void VisitBreak(BreakNode node) {
            UpdateLine(node);
            asm.Jump(loop.End);
        }

        public void VisitContinue(ContinueNode node) {
            UpdateLine(node);
            asm.Jump(loop.CodeEnd);
        }

        public void VisitImport(ImportNode node) {
        	string alias = null;
        	if(node.Alias != null) {
        		alias = node.Alias.Value;
        	}
        	chunk.Imports.Add(new Import(node.Path.Value, alias));
		}

        public void VisitReturn(ReturnNode node) {
            UpdateLine(node);
            node.Value.Accept(this);
            asm.Return(true);
        }

        public void VisitDefer(DeferNode node) {
            UpdateLine(node);
            NewProcedure($"deferred_lambda{node.ID}", false, false, node.Position.Line, false);
            UpdateLine(node);

            node.Code.Accept(this);
            asm.Return();

            var pasm = asm;
            var proc = FinishProcedure();

            Closure(proc);

            asm.Defer();
        }

        public void VisitSlice(SliceNode node) {
            UpdateLine(node);
            node.Array.Accept(this);
            node.Start.Accept(this);
            node.End.Accept(this);
            asm.Slice();
        }

        public void VisitWhile(WhileNode node) {
            UpdateLine(node);
            PushScope();

            var start = asm.NewLabel();
            asm.MarkLabel(start);
            node.Condition.Accept(this);
            var end = asm.NewLabel();
            asm.Not();
            asm.Branch(end);
            var codeEnd = asm.NewLabel();
            PushLoop(start, end, codeEnd);
            node.Code.Accept(this);
            asm.MarkLabel(codeEnd);
            asm.Jump(start);
            asm.MarkLabel(end);
            PopLoop();

            PopScope();
        }

        public void VisitExpressionStatement(ExpressionStatementNode node) {
            UpdateLine(node);
            node.Value.Accept(this);
            asm.Pop();
        }

        public void VisitVar(VarNode node) {
            UpdateLine(node);

            var flags = VariableFlags.NONE;
            if(node.Exported) flags |= VariableFlags.EXPORTED;
            if(node.Final) flags |= VariableFlags.FINAL;

            if (chunk.Initializer == null) {
                chunk.Variables[node.Name.Value] = new Variable(node.Name.Value, flags);
            } else {
                asm.Declare(node.Name.Value, flags);
                asm.MarkLocal(node.Name.Value);
            }

            if (node.DefaultValue != null) {
                node.DefaultValue.Accept(this);

                if (chunk.Initializer == null) asm.SetGlobal(node.Name.Value);
                else asm.SetLocal(node.Name.Value);
            }
        }

        public void VisitIf(IfNode node) {
            UpdateLine(node);
            node.Condition.Accept(this);
            var label = asm.NewLabel();
            asm.Branch(label);
            if (node.False != null) {
                node.False.Accept(this);
            }
            var end = asm.NewLabel();
            asm.Jump(end);
            asm.MarkLabel(label);
            node.True.Accept(this);
            asm.MarkLabel(end);
        }

        public void VisitDo(DoNode node) {
            UpdateLine(node);
            PushScope();

            var start = asm.NewLabel();
            var end = asm.NewLabel();
            var codeEnd = asm.NewLabel();
            asm.MarkLabel(start);
            PushLoop(start, end, codeEnd);
            node.Code.Accept(this);
            asm.MarkLabel(codeEnd);
            node.Condition.Accept(this);
            asm.Branch(start);
            PopLoop();

            PopScope();
        }

        public void VisitArray(ArrayNode node) {
            UpdateLine(node);
            asm.NewArray();
            foreach (var e in node.DefaultElements) {
                asm.Dup();
                e.Accept(this);
                asm.ArrayAdd();
            }
        }

        public void VisitParen(ParenNode node) {
            node.Value.Accept(this);
        }

        public void VisitString(StringNode node) {
            UpdateLine(node);
            asm.PushConstant(NeoString.ValueOf(node.Value));
        }

        public void VisitForRange(ForRangeNode node) {
            UpdateLine(node);
            PushScope();

            asm.Declare(node.Iterator.Value, VariableFlags.NONE);
            node.Start.Accept(this);
            asm.MarkLocal(node.Iterator.Value);
            asm.SetLocal(node.Iterator.Value);

            var endValue = "__end_" + Guid.NewGuid().ToString("n");
            asm.Declare(endValue, VariableFlags.FINAL);
            node.End.Accept(this);
            asm.MarkLocal(endValue);
            asm.SetLocal(endValue);

            var byValue = "__by_" + Guid.NewGuid().ToString("n");
            asm.Declare(byValue, VariableFlags.FINAL);
            if(node.By != null) {
                node.By.Accept(this);
            } else {
                asm.PushConstant(NeoInt.ValueOf(1));
            }
            asm.MarkLocal(byValue);
            asm.SetLocal(byValue);

            var cmpValue = "__cmp_" + Guid.NewGuid().ToString("n");
            asm.Declare(cmpValue, VariableFlags.NONE);
            asm.MarkLocal(cmpValue);
            asm.PushConstant(NeoInt.ValueOf(1));
            asm.SetLocal(cmpValue);

            var skip = asm.NewLabel();
            asm.GetLocal(byValue);
            asm.PushConstant(NeoInt.ValueOf(0));
            asm.Lte();
            asm.Branch(skip);
            asm.PushConstant(NeoInt.ValueOf(-1));
            asm.SetLocal(cmpValue);
            asm.MarkLabel(skip);

            var start = asm.NewLabel();
            asm.MarkLabel(start);
            asm.GetLocal(node.Iterator.Value);
            asm.GetLocal(endValue);
            asm.Cmp();
            asm.GetLocal(cmpValue);
            asm.Eq();
            var end = asm.NewLabel();
            var codeEnd = asm.NewLabel();
            PushLoop(start, end, codeEnd);
            asm.Branch(end);
            node.Code.Accept(this);
            asm.MarkLabel(codeEnd);
            // CloseScope();
            asm.GetLocal(node.Iterator.Value);
            asm.GetLocal(byValue);
            asm.Add();
            asm.SetLocal(node.Iterator.Value);
            asm.Jump(start);
            asm.MarkLabel(end);
            // CloseScope();
            PopLoop();

            PopScope();
        }

        public void VisitForIterator(ForIteratorNode node) {
            UpdateLine(node);
            PushScope();

            asm.Declare(node.Iterator.Value, VariableFlags.NONE);
            asm.MarkLocal(node.Iterator.Value);

            var idx = "__idx_" + Guid.NewGuid().ToString("n");
            asm.Declare(idx, VariableFlags.NONE);
            asm.MarkLocal(idx);
            asm.PushConstant(NeoInt.ValueOf(0));
            asm.SetLocal(idx);

            var from = "__from_" + Guid.NewGuid().ToString("n");
            asm.Declare(from, VariableFlags.NONE);
            asm.MarkLocal(from);
            node.From.Accept(this);
            asm.SetLocal(from);
            
            var start = asm.NewLabel();
            asm.MarkLabel(start);
            asm.GetLocal(idx);
            asm.GetLocal(from);
            asm.Length();
            asm.Eq();
            var end = asm.NewLabel();
            PushLoop(start, end, start);
            asm.Branch(end);
            asm.GetLocal(from);
            asm.GetLocal(idx);
            asm.Get();
            asm.SetLocal(node.Iterator.Value);
            asm.GetLocal(idx);
            asm.Inc();
            asm.SetLocal(idx);
            if(!(node.Code is BlockNode)) PushUpValueTracking();
            node.Code.Accept(this);
            if(!(node.Code is BlockNode)) {
                var ups = PopUpValueTracking();
                foreach(var up in ups) {
                    asm.Close(up);
                }
            }
            asm.Jump(start);
            asm.MarkLabel(end);
            PopLoop();

            PopScope();
        }

        public void VisitForKeyValueIterator(ForKeyValueIteratorNode node) {
            UpdateLine(node);
            PushScope();

            asm.Declare(node.Key.Value, VariableFlags.NONE);
            asm.MarkLocal(node.Value.Value);
            asm.Declare(node.Value.Value, VariableFlags.NONE);
            asm.MarkLocal(node.Key.Value);

            var idx = "__idx_" + Guid.NewGuid().ToString("n");
            asm.Declare(idx, VariableFlags.NONE);
            asm.MarkLocal(idx);
            asm.PushConstant(NeoInt.ValueOf(0));
            asm.SetLocal(idx);

            var from = "__from_" + Guid.NewGuid().ToString("n");
            asm.Declare(from, VariableFlags.NONE);
            asm.MarkLocal(from);
            node.From.Accept(this);
            asm.SetLocal(from);

            var start = asm.NewLabel();
            asm.MarkLabel(start);
            asm.GetLocal(idx);
            asm.GetLocal(from);
            asm.Length();
            asm.Eq();
            var end = asm.NewLabel();
            PushLoop(start, end, start);
            asm.Branch(end);
            asm.GetLocal(from);
            asm.GetLocal(idx);
            asm.ObjectIndex();
            asm.SetLocal(node.Value.Value);
            asm.SetLocal(node.Key.Value);
            asm.GetLocal(idx);
            asm.Inc();
            asm.SetLocal(idx);
            if(!(node.Code is BlockNode)) PushUpValueTracking();
            node.Code.Accept(this);
            if(!(node.Code is BlockNode)) {
                PopUpValueTrackingAndClose();
            }
            asm.Jump(start);
            asm.MarkLabel(end);
            PopLoop();

            PopScope();
        }

        private void GenerateIncrementOrDecrement(UnaryNode node, bool increment, bool postfix) {
            if(postfix) asm.Dup();

            if(increment) asm.Inc();
            else          asm.Dec();

            if(!postfix) asm.Dup();

            if (node.Value is IndexNode l) {
                l.Value.Accept(this);
                asm.Swap();
                l.Key.Accept(this);
                asm.Swap();
                asm.Set();
            } else if (node.Value is IdentNode i) {
                if (asm.IsLocal(i.Value)) {
                    asm.SetLocal(i.Value);
                } else if (asm.IsUpValue(i.Value)) {
                    SetUpValue(i.Value);
                } else {
                    asm.SetGlobal(i.Value);
                }
            } else {
                throw new Exception($"Unexpected node: {node.Value}");
            }
        }

        public void VisitUnary(UnaryNode node) {
            UpdateLine(node);
            node.Value.Accept(this);

            switch (node.OP) {
                case UnaryOP.BIT_NOT:
                    asm.BitNot();
                    break;
                case UnaryOP.NOT:
                    asm.Not();
                    break;
                case UnaryOP.LENGTH:
                    asm.Length();
                    break;
                case UnaryOP.NEG:
                    asm.Neg();
                    break;
                case UnaryOP.UNPACK:
                    asm.Unpack();
                    break;
                case UnaryOP.FROZEN:
                    asm.Frozen();
                    break;
                case UnaryOP.PREFIX_INCREMENT:
                    GenerateIncrementOrDecrement(node, true, false);
                    break;
                case UnaryOP.PREFIX_DECREMENT:
                    GenerateIncrementOrDecrement(node, false, false);
                    break;                
                case UnaryOP.POSTFIX_INCREMENT:
                    GenerateIncrementOrDecrement(node, true, true);
                    break;
                case UnaryOP.POSTFIX_DECREMENT:
                    GenerateIncrementOrDecrement(node, false, true);
                    break;
                default: {
                        throw new Exception(node.OP.ToString());
                    }
            }
        }

        public void VisitNil(NilNode node) {
            UpdateLine(node);
            asm.PushNil();
        }

        public void VisitAssign(AssignNode node) {
            UpdateLine(node);
            if (node.OP == AssignOP.NORMAL) {
                if (node.Left is IndexNode l) {
                    l.Value.Accept(this);
                    l.Key.Accept(this);
                    node.Right.Accept(this);
                    asm.Set();
                } else if (node.Left is IdentNode i) {
                    node.Right.Accept(this);
                    if (asm.IsLocal(i.Value)) {
                        asm.SetLocal(i.Value);
                    } else if (asm.IsUpValue(i.Value)) {
                        SetUpValue(i.Value);
                    } else {
                        asm.SetGlobal(i.Value);
                    }
                } else {
                    throw new Exception($"Unexpected node: {node.Left}");
                }
            } else {
                if (node.Left is IndexNode l) {
                    l.Value.Accept(this);
                    l.Key.Accept(this);
                }

                if (node.OP == AssignOP.AND) {
                    node.Left.Accept(this);
                    var f = asm.NewLabel();
                    asm.Not();
                    asm.Branch(f);
                    node.Right.Accept(this);
                    asm.Not();
                    asm.Branch(f);
                    asm.PushTrue();
                    var end = asm.NewLabel();
                    asm.Jump(end);
                    asm.MarkLabel(f);
                    asm.PushFalse();
                    asm.MarkLabel(end);
                } else if (node.OP == AssignOP.OR) {
                    node.Left.Accept(this);
                    var t = asm.NewLabel();
                    asm.Branch(t);
                    node.Right.Accept(this);
                    asm.Branch(t);
                    asm.PushFalse();
                    var end = asm.NewLabel();
                    asm.Jump(end);
                    asm.MarkLabel(t);
                    asm.PushTrue();
                    asm.MarkLabel(end);
                } else {
                    node.Left.Accept(this);
                    node.Right.Accept(this);

                    switch (node.OP) {
                        case AssignOP.ADD: {
                                asm.Add();
                            }
                            break;
                        case AssignOP.SUB: {
                                asm.Sub();
                            }
                            break;
                        case AssignOP.MUL: {
                                asm.Mul();
                            }
                            break;
                        case AssignOP.DIV: {
                                asm.Div();
                            }
                            break;
                        case AssignOP.POW: {
                                asm.Pow();
                            }
                            break;
                        case AssignOP.MOD: {
                                asm.Mod();
                            }
                            break;
                        case AssignOP.LSH: {
                                asm.Lsh();
                            }
                            break;
                        case AssignOP.RSH: {
                                asm.Rsh();
                            }
                            break;
                        case AssignOP.BIT_NOT: {
                                asm.BitNot();
                            }
                            break;
                        case AssignOP.BIT_AND: {
                                asm.BitAnd();
                            }
                            break;
                        case AssignOP.BIT_OR: {
                                asm.BitOr();
                            }
                            break;
                        case AssignOP.BIT_XOR: {
                                asm.BitXor();
                            }
                            break;
                        case AssignOP.CONCAT: {
                                asm.Concat();
                            }
                            break;
                        default: {
                                throw new Exception(node.OP.ToString());
                            }
                    }
                }

                if (node.Left is IndexNode) {
                    asm.Set();
                } else if (node.Left is IdentNode i) {
                    if (asm.IsLocal(i.Value)) {
                        asm.SetLocal(i.Value);
                    } else if (asm.IsUpValue(i.Value)) {
                        SetUpValue(i.Value);
                    } else {
                        asm.SetGlobal(i.Value);
                    }
                } else {
                    throw new Exception($"Unexpected node: {node.Left}");
                }
            }
        }

        public void VisitInt(IntNode node) {
            UpdateLine(node);
            asm.PushConstant(NeoInt.ValueOf(node.Value));
        }

        public void VisitIndex(IndexNode node) {
            UpdateLine(node);
            node.Value.Accept(this);
            node.Key.Accept(this);
            asm.Get();
        }

        public void VisitBlock(BlockNode node) {
            UpdateLine(node);
            PushScope();

            PushUpValueTracking();
            foreach (var s in node.Statements) {
                s.Accept(this);
            }
            var ups = PopUpValueTracking();

            foreach(var up in ups) {
                asm.Close(up);
            }

            PopScope();
        }

        public void VisitIdent(IdentNode node) {
            UpdateLine(node);
            if (asm.IsLocal(node.Value)) {
                asm.GetLocal(node.Value);
            } else if (asm.IsUpValue(node.Value)) {
                GetUpValue(node.Value);
            } else {
                asm.GetGlobal(node.Value);
            }
        }

        public void VisitBinary(BinaryNode node) {
            UpdateLine(node);
            if (node.OP == BinaryOP.AND) {
                node.Left.Accept(this);
                var f = asm.NewLabel();
                asm.Not();
                asm.Branch(f);
                node.Right.Accept(this);
                asm.Not();
                asm.Branch(f);
                asm.PushTrue();
                var end = asm.NewLabel();
                asm.Jump(end);
                asm.MarkLabel(f);
                asm.PushFalse();
                asm.MarkLabel(end);
            } else if (node.OP == BinaryOP.OR) {
                node.Left.Accept(this);
                var t = asm.NewLabel();
                asm.Branch(t);
                node.Right.Accept(this);
                asm.Branch(t);
                asm.PushFalse();
                var end = asm.NewLabel();
                asm.Jump(end);
                asm.MarkLabel(t);
                asm.PushTrue();
                asm.MarkLabel(end);
            } else {
                node.Left.Accept(this);
                node.Right.Accept(this);

                switch (node.OP) {
                    case BinaryOP.ADD:
                        asm.Add();
                        break;
                    case BinaryOP.SUB:
                        asm.Sub();
                        break;
                    case BinaryOP.MUL:
                        asm.Mul();
                        break;
                    case BinaryOP.DIV:
                        asm.Div();
                        break;
                    case BinaryOP.POW:
                        asm.Pow();
                        break;
                    case BinaryOP.MOD:
                        asm.Mod();
                        break;
                    case BinaryOP.LSH:
                        asm.Lsh();
                        break;
                    case BinaryOP.RSH:
                        asm.Rsh();
                        break;
                    case BinaryOP.BIT_NOT:
                        asm.BitNot();
                        break;
                    case BinaryOP.BIT_AND:
                        asm.BitAnd();
                        break;
                    case BinaryOP.BIT_OR:
                        asm.BitOr();
                        break;
                    case BinaryOP.BIT_XOR:
                        asm.BitXor();
                        break;
                    case BinaryOP.CONCAT:
                        asm.Concat();
                        break;
                    case BinaryOP.EQ:
                        asm.Eq();
                        break;
                    case BinaryOP.NE:
                        asm.Ne();
                        break;
                    case BinaryOP.DEEP_EQ:
                        asm.DeepEq();
                        break;
                    case BinaryOP.DEEP_NE:
                        asm.DeepNe();
                        break;
                    case BinaryOP.LT:
                        asm.Lt();
                        break;
                    case BinaryOP.GT:
                        asm.Gt();
                        break;
                    case BinaryOP.LTE:
                        asm.Lte();
                        break;
                    case BinaryOP.GTE:
                        asm.Gte();
                        break;
                    default:
                        throw new Exception(node.OP.ToString());
                }
            }
        }

        public void VisitFloat(FloatNode node) {
            UpdateLine(node);
            asm.PushConstant(NeoFloat.ValueOf(node.Value));
        }

        public void VisitBool(BoolNode node) {
            UpdateLine(node);
            if (node.Value) {
                asm.PushTrue();
            } else {
                asm.PushFalse();
            }
        }

        public void VisitCall(CallNode node) {
            UpdateLine(node);
            node.Proc.Accept(this);
            node.Arguments.ForEach(a => a.Accept(this));
            asm.Call((short)node.Arguments.Count);
        }

        public void VisitObject(ObjectNode node) {
            UpdateLine(node);
            asm.NewObject();

            var elems = node.DefaultElements;
            foreach (var d in elems) {
                asm.Dup();
                d.Item1.Accept(this);
                d.Item2.Accept(this);
                asm.Set();
            }
        }

        public void VisitLambda(LambdaNode node) {
            UpdateLine(node);
            NewProcedure(node.Name, false, node.Varargs, node.Position.Line, false);
            UpdateLine(node);

            node.Parameters.ForEach(p => p.Accept(this));
            node.Statements.ForEach(s => s.Accept(this));

            if (node.Statements.Count == 0 || !(node.Statements.Last() is ReturnNode)) {
                asm.Return();
            }

            Closure(FinishProcedure());
        }

        public void VisitVarargs(VarargsNode node) {
            UpdateLine(node);
            asm.Varargs();
        }

        public void VisitTernary(TernaryNode node) {
            UpdateLine(node);
            var label = asm.NewLabel();
            node.Condition.Accept(this);
            asm.Branch(label);
            node.False.Accept(this);
            var end = asm.NewLabel();
            asm.Jump(end);
            asm.MarkLabel(label);
            node.True.Accept(this);
            asm.MarkLabel(end);
        }

        public void VisitTryCatch(TryCatchNode node) {
            UpdateLine(node);

            NewProcedure($"try{node.ID}", false, false, -1, false);
            UpdateLine(node);
            node.Try.Accept(this);
            asm.Return();
            var tryProc = FinishProcedure();

            NewProcedure($"catch{node.ID}", false, false, -1, false);
            UpdateLine(node);

            asm.AddParameter(node.Error.Value, false);
            asm.MarkLocal(node.Error.Value);

            node.Catch.Accept(this);
            asm.Return();
            var catchProc = FinishProcedure();

            Closure(tryProc);
            Closure(catchProc);
            asm.Try();
        }

        public void VisitThrow(ThrowNode node) {
            UpdateLine(node);
            node.Value.Accept(this);
            asm.Throw();
        }

        public void VisitEnum(EnumNode node) {
            UpdateLine(node);
            asm.NewObject();

            var elements = node.Elements.ToArray();
            for (var i = 0; i < elements.Length; i++) {
                asm.Dup();
                asm.PushConstant(NeoString.ValueOf(elements[i]));
                asm.PushConstant(NeoInt.ValueOf(i));
                asm.Set();

                asm.Dup();
                asm.PushConstant(NeoInt.ValueOf(i));
                asm.PushConstant(NeoString.ValueOf(elements[i]));
                asm.Set();
            }

            asm.Frozen();
        }

        public void VisitEnumDeclaration(EnumDeclarationNode node) {
            UpdateLine(node);

            var flags = VariableFlags.FINAL;
            if(node.Exported) flags |= VariableFlags.EXPORTED;
            
            if (chunk.Initializer == null) {
                chunk.Variables[node.Name] = new Variable(node.Name, flags);
            } else {
                asm.Declare(node.Name, flags);
                asm.MarkLocal(node.Name);
            }

            node.Enum.Accept(this);

            if (chunk.Initializer == null) asm.SetGlobal(node.Name);
            else asm.SetLocal(node.Name);
        }

        public void VisitParameter(ParameterNode node) {
            asm.AddParameter(node.Name.Value, node.Frozen);
            asm.MarkLocal(node.Name.Value);
        }

        private sealed class Loop {
            public Label Start { get; set; }

            public Label End { get; set; }

            public Label CodeEnd { get; set; }
        }
    }
}
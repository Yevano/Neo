using Neo.Runtime;
using Neo.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.Bytecode {
    public sealed class ProcedureWriter {
        private readonly string name;
        private readonly bool exported;
        private readonly bool varargs;
        private readonly int line;

        private int currentLine = -1;
        private readonly List<int> lines;
        private readonly List<int> instructionAddresses;

        private readonly Pool<NeoValue> constants;
        private readonly MemoryStream stream;
        private readonly BinaryWriter writer;
        private readonly List<Label> labels;
        private readonly Pool<Procedure> procedures;
        private readonly List<Parameter> parameters;
        private readonly HashSet<string> localNames;
        private readonly HashSet<string> upvalueNames;
        private readonly HashSet<string> usedUpvalues;
        private readonly List<Patch> patches;

        private readonly int numInstructionsPosition;
        private int numInstructions;

        public ProcedureWriter(string name, bool exported, bool varargs, int line) {
            this.name = name;
            this.exported = exported;
            this.varargs = varargs;
            this.line = line;
            
            lines = new List<int>();
            instructionAddresses = new List<int>();

            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
            constants = new Pool<NeoValue>();
            labels = new List<Label>();
            procedures = new Pool<Procedure>();
            parameters = new List<Parameter>();
            localNames = new HashSet<string>();
            upvalueNames = new HashSet<string>();
            usedUpvalues = new HashSet<string>();
            patches = new List<Patch>();

            numInstructionsPosition = (int)stream.Position;
            writer.Write(-1);
        }

        private void WriteConstantType(ConstantType type) {
            writer.Write((byte)type);
        }

        private void WriteConstant(NeoValue value) {
            writer.Write(constants[value]);
        }

        private void WriteOpCode(OpCode op) {
            if(currentLine == -1) {
                throw new Exception();
            }

            numInstructions++;
            lines.Add(currentLine);
            instructionAddresses.Add((int)stream.Position);
            writer.Write((byte)op);
        }

        private List<LineRange> CalculateLineRanges() {
            var result = new List<LineRange>();

            var addedLast = false;

            var start = 0;
            var prev = lines[start];
            for(var i = 0; i < lines.Count; i++) {
                var line = lines[i];
                if(line == prev) continue;
                
                result.Add(new LineRange(prev, instructionAddresses[start], instructionAddresses[i - 1]));
                if(i == lines.Count - 1) addedLast = true;

                start = i;
                prev = line;
            }

            if(!addedLast) result.Add(new LineRange(prev, instructionAddresses[start], instructionAddresses[instructionAddresses.Count - 1]));

            return result;
        }

        public IEnumerable<Parameter> Parameters => parameters;

        public IEnumerable<string> Locals => localNames;

        public IEnumerable<string> UpValues => upvalueNames;

        public IEnumerable<string> UsedUpValues => usedUpvalues;

        public Procedure Finish() {
            var end = (int)writer.BaseStream.Position;

            foreach (var patch in patches) {
                var label = labels[patch.Index];
                if (label.PC == -1) {
                    throw new Exception($"Unvisited label {label.Index}");
                }
                stream.Seek(patch.Address, SeekOrigin.Begin);
                writer.Write(label.PC);
            }

            writer.Seek(numInstructionsPosition, SeekOrigin.Begin);
            writer.Write(numInstructions);

            writer.Seek(end, SeekOrigin.Begin);

            return new Procedure(name, exported, usedUpvalues.ToArray(), parameters.ToArray(), varargs, CalculateLineRanges().ToArray(), line, stream.ToArray(), constants.ToArray(), procedures.ToArray());
        }

        public void AddProcedure(Procedure procedure) {
            procedures.Add(procedure);
        }

        public Label NewLabel() {
            var label = new Label() {
                Index = labels.Count
            };
            labels.Add(label);
            return label;
        }

        public void AddParameter(string name, bool frozen) => parameters.Add(new Parameter(name, frozen));

        public bool IsParameter(string name) => parameters.Select(p => p.Name).Contains(name);

        public void MarkLocal(string name) {
            if (!localNames.Contains(name)) {
                localNames.Add(name);
            }
        }

        public bool IsLocal(string name) => localNames.Contains(name);

        public void MarkUpValue(string name) {
            if (!upvalueNames.Contains(name)) {
                upvalueNames.Add(name);
            }
        }

        public bool IsUpValue(string name) {
            return upvalueNames.Contains(name);
        }

        public void MarkLabel(Label label) {
            label.PC = (int)writer.BaseStream.Position;
        }

        public void SetLine(int line) {
            currentLine = line;
        }

        public void Nop() {
            WriteOpCode(OpCode.NOP);
        }

        public void Inc() {
            WriteOpCode(OpCode.INC);
        }

        public void Dec() {
            WriteOpCode(OpCode.DEC);
        }

        public void Add() {
            WriteOpCode(OpCode.ADD);
        }

        public void Sub() {
            WriteOpCode(OpCode.SUB);
        }

        public void Mul() {
            WriteOpCode(OpCode.MUL);
        }

        public void Div() {
            WriteOpCode(OpCode.DIV);
        }

        public void Pow() {
            WriteOpCode(OpCode.POW);
        }

        public void Mod() {
            WriteOpCode(OpCode.MOD);
        }

        public void Lsh() {
            WriteOpCode(OpCode.LSH);
        }

        public void Rsh() {
            WriteOpCode(OpCode.RSH);
        }

        public void BitNot() {
            WriteOpCode(OpCode.BIT_NOT);
        }

        public void BitAnd() {
            WriteOpCode(OpCode.BIT_AND);
        }

        public void BitOr() {
            WriteOpCode(OpCode.BIT_OR);
        }

        public void BitXor() {
            WriteOpCode(OpCode.BIT_XOR);
        }

        public void Not() {
            WriteOpCode(OpCode.NOT);
        }

        public void Neg() {
            WriteOpCode(OpCode.NEG);
        }

        public void Concat() {
            WriteOpCode(OpCode.CONCAT);
        }

        public void Length() {
            WriteOpCode(OpCode.LENGTH);
        }

        public void NewArray() {
            WriteOpCode(OpCode.ARRAY_NEW);
        }

        public void ArrayAdd() {
            WriteOpCode(OpCode.ARRAY_ADD);
        }

        public void NewObject() {
            WriteOpCode(OpCode.OBJECT_NEW);
        }

        public void ObjectIndex() {
            WriteOpCode(OpCode.OBJECT_INDEX);
        }

        public void Get() {
            WriteOpCode(OpCode.GET);
        }

        public void Set() {
            WriteOpCode(OpCode.SET);
        }

        public void Slice() {
            WriteOpCode(OpCode.SLICE);
        }

        public void Eq() {
            WriteOpCode(OpCode.EQ);
        }

        public void Ne() {
            WriteOpCode(OpCode.NE);
        }

        public void DeepEq() {
            WriteOpCode(OpCode.DEEP_EQ);
        }

        public void DeepNe() {
            WriteOpCode(OpCode.DEEP_NE);
        }

        public void Lt() {
            WriteOpCode(OpCode.LT);
        }

        public void Gt() {
            WriteOpCode(OpCode.GT);
        }

        public void Lte() {
            WriteOpCode(OpCode.LTE);
        }

        public void Gte() {
            WriteOpCode(OpCode.GTE);
        }

        public void Cmp() {
            WriteOpCode(OpCode.CMP);
        }

        public void Jump(Label label) {
            WriteOpCode(OpCode.JUMP);
            patches.Add(new Patch() {
                Index = label.Index,
                Address = (int)stream.Position
            });
            writer.Write(label.Index);
        }

        public void Branch(Label label) {
            WriteOpCode(OpCode.JUMP_IF);
            patches.Add(new Patch() {
                Index = label.Index,
                Address = (int)stream.Position
            });
            writer.Write(label.Index);
        }

        public void Call(short arguments) {
            WriteOpCode(OpCode.CALL);
            writer.Write(arguments);
        }

        public void Return(bool value = false) {
            WriteOpCode(OpCode.RETURN);
            writer.Write(value);
        }

        public void Defer() {
            WriteOpCode(OpCode.DEFER);
        }

        public void Varargs() {
            WriteOpCode(OpCode.VARARGS);
        }

        public void PushTrue() {
            WriteOpCode(OpCode.PUSH_TRUE);
        }

        public void PushFalse() {
            WriteOpCode(OpCode.PUSH_FALSE);
        }

        public void PushNil() {
            WriteOpCode(OpCode.PUSH_NIL);
        }

        public void PushConstant(NeoValue constant) {
            if (constant.IsBool) {
                WriteOpCode(constant.CheckBool().Value ? OpCode.PUSH_TRUE : OpCode.PUSH_FALSE);
            } else if (constant.IsNil) {
                WriteOpCode(OpCode.PUSH_NIL);
            } else {
                WriteOpCode(OpCode.PUSH_CONSTANT);
                writer.Write(constants[constant]);
            }
        }

        public void Dup() {
            WriteOpCode(OpCode.DUP);
        }

        public void Swap() {
            WriteOpCode(OpCode.SWAP);
        }

        public void Unpack() {
            WriteOpCode(OpCode.SPREAD); 
        }

        public void Frozen() {
            WriteOpCode(OpCode.FROZEN);
        }

        public void Pop() {
            WriteOpCode(OpCode.POP);
        }

        public void Closure(string proc) {
            WriteOpCode(OpCode.CLOSURE);
            writer.Write(constants[NeoString.ValueOf(proc)]);
        }

        public void Close(string name) {
            WriteOpCode(OpCode.CLOSE);
            writer.Write(constants[NeoString.ValueOf(name)]);
        }

        public void GetLocal(string name) {
            WriteOpCode(OpCode.GET_LOCAL);
            writer.Write(constants[NeoString.ValueOf(name)]);
        }

        public void SetLocal(string name) {
            WriteOpCode(OpCode.SET_LOCAL);
            writer.Write(constants[NeoString.ValueOf(name)]);
        }

        public void GetGlobal(string name) {
            WriteOpCode(OpCode.GET_GLOBAL);
            writer.Write(constants[NeoString.ValueOf(name)]);
        }

        public void SetGlobal(string name) {
            WriteOpCode(OpCode.SET_GLOBAL);
            writer.Write(constants[NeoString.ValueOf(name)]);
        }

        public void GetUpValue(string name) {
            if (!usedUpvalues.Contains(name)) {
                usedUpvalues.Add(name);
            }

            WriteOpCode(OpCode.GET_UPVALUE);
            writer.Write(constants[NeoString.ValueOf(name)]);
        }

        public void SetUpValue(string name) {
            if (!usedUpvalues.Contains(name)) {
                usedUpvalues.Add(name);
            }

            WriteOpCode(OpCode.SET_UPVALUE);
            writer.Write(constants[NeoString.ValueOf(name)]);
        }

        public void Try() {
            WriteOpCode(OpCode.TRY);
        }

        public void Throw() {
            WriteOpCode(OpCode.THROW);
        }

        public void Declare(string name, VariableFlags flags) {
            WriteOpCode(OpCode.DECLARE);
            writer.Write(constants[NeoString.ValueOf(name)]);
        	writer.Write((byte)flags);
        }

        internal sealed class Patch {
            public int Index;

            public int Address;
        }
    }

    public sealed class Label {
        public int Index;

        public int PC = -1;

        internal Label() {
        }
    }
}
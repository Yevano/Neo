using System.Collections.Generic;
using Neo.Bytecode;
using Neo.Runtime;
using Neo.Runtime.Internal;

namespace Neo.Backend.JIT {
    internal sealed class JITProcedure : IProcedureImplementation {
        private readonly VM vm;
        private readonly Scope parentScope;
        private readonly Chunk chunk;
        private readonly Procedure procedure;
        private readonly UpValue[] upvalues;
        private readonly ProcedureCall @delegate;

        public JITProcedure(VM vm, Scope parentScope, Chunk chunk, Procedure procedure, UpValue[] upvalues, ProcedureCall @delegate) {
            this.vm = vm;
            this.parentScope = parentScope;
            this.chunk = chunk;
            this.procedure = procedure;
            this.upvalues = upvalues;
            this.@delegate = @delegate;
        }

        public NeoValue Call(NeoValue[] arguments) {
            var scope = new Scope(parentScope);

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
            var extraArgs = pargs.Count - procedure.Parameters.Length;
            if (extraArgs > 0) {
                for (var i = 0; i < extraArgs; i++) {
                    varargs.Insert(pargs[procedure.Parameters.Length + i]);
                }
            }

            var upvalues = new Dictionary<string, UpValue>();
            foreach (var upvalue in this.upvalues) {
                upvalues[upvalue.Name] = upvalue;
            }

            foreach (var parameter in procedure.Parameters) {
                scope.Declare(parameter.Name, VariableFlags.NONE);
            }

            return @delegate(scope, pargs.ToArray(), varargs, upvalues, vm, parentScope, chunk, procedure);
        }

        public string Name() => procedure.Name;

        public string ChunkName() => chunk.Name;

        public static void CloseAll(Dictionary<string, UpValue> openUps) {
            foreach (var up in openUps.Values) {
                up.Close();
            }
        }

        public static void RunDefers(Stack<NeoProcedure> defers) {
            while (defers.Count > 0) {
                defers.Pop().Call(new NeoValue[0]);
            }
        }

        public static void Close(Dictionary<string, UpValue> openUps, string name) {
            if (openUps.ContainsKey(name)) {
                openUps[name].Close();
                openUps.Remove(name);
            }
        }

        public static NeoValue TryCatch(NeoProcedure @try, NeoProcedure @catch) {
            try {
                return @try.Call(new NeoValue[0]);
            } catch (NeoError e) {
                return @catch.Call(new[] { NeoString.ValueOf(e.Message) });
            }
        }
    }
}
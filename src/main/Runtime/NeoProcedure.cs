using System;

namespace Neo.Runtime {
    public abstract class NeoProcedure : NeoValue {
        public static NeoProcedure FromLambda(NeoProcedureFunc lambda) {
            return new NativeNeoProcedure(lambda);
        }

        public abstract override NeoValue Call(NeoValue[] arguments);

        public abstract string Name();

        public abstract string ChunkName();

        public override string Type => "procedure";
    }

    internal class NativeNeoProcedure : NeoProcedure {
        private NeoProcedureFunc lambda;

        public NativeNeoProcedure(NeoProcedureFunc lambda) {
            this.lambda = lambda;
        }

        public override string Name() => "lambda";

        public override string ChunkName() => "<native code>";

        public override NeoValue Call(NeoValue[] arguments) {
            return lambda(arguments);
        }
    }

    public delegate NeoValue NeoProcedureFunc(NeoValue[] args);
}
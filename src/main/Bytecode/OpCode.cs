namespace Neo.Bytecode {
    public enum OpCode : byte {
        NOP,

        INC,
        DEC,

        ADD,
        SUB,
        MUL,
        DIV,
        POW,
        MOD,

        LSH,
        RSH,

        BIT_NOT,
        BIT_AND,
        BIT_OR,
        BIT_XOR,

        NOT,
        NEG,
        CONCAT,
        LENGTH,

        ARRAY_NEW,
        ARRAY_ADD,
        OBJECT_NEW,
        OBJECT_INDEX,
        GET,
        SET,
        SLICE,

        EQ,
        NE,
        DEEP_EQ,
        DEEP_NE,
        LT,
        GT,
        LTE,
        GTE,

        CMP,

        JUMP,
        JUMP_IF,

        CALL,
        RETURN,

        DEFER,
        VARARGS,
        SPREAD,
        FROZEN,

        PUSH_TRUE,
        PUSH_FALSE,
        PUSH_NIL,
        PUSH_CONSTANT,

        DUP,
        SWAP,
        POP,

        CLOSURE,
        CLOSE,

        GET_LOCAL,
        SET_LOCAL,
        GET_GLOBAL,
        SET_GLOBAL,
        GET_UPVALUE,
        SET_UPVALUE,

        TRY,
        THROW,
        DECLARE
    }
}
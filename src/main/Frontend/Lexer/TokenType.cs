namespace Neo.Frontend.Lexer {
    public enum TokenType {
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        LBRACK,
        RBRACK,

        SEMICOLON,
        COLON,
        COMMA,
        PERIOD,
        UNPACK,
        QUESTION_MARK,

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

        AND,
        OR,
        NOT,

        CONCAT,

        ASSIGN,

        ADD_ASSIGN,
        SUB_ASSIGN,
        MUL_ASSIGN,
        DIV_ASSIGN,
        POW_ASSIGN,
        MOD_ASSIGN,

        LSH_ASSIGN,
        RSH_ASSIGN,

        BIT_NOT_ASSIGN,
        BIT_AND_ASSIGN,
        BIT_OR_ASSIGN,
        BIT_XOR_ASSIGN,

        AND_ASSIGN,
        OR_ASSIGN,

        CONCAT_ASSIGN,

        LENGTH,

        EQ,
        NE,
        DEEP_EQ,
        DEEP_NE,
        LT,
        GT,
        LTE,
        GTE,

        FAT_ARROW,

        VARARGS,

        EXPORTED,
        VAR,
        IF,
        ELSE,
        BREAK,
        CONTINUE,
        FOR,
        IN,
        TO,
        BY,
        WHILE,
        DO,
        DEFER,
        RETURN,
        PROC,
        IMPORT,
        AS,
        TRY,
        CATCH,
        THROW,
        FINAL,
        FROZEN,
        ENUM,

        TRUE,
        FALSE,
        NIL,

        INTEGER,
        FLOAT,
        STRING,
        HERE_STRING,
        INTERPOLATED_STRING,
        IDENT,

        SHEBANG,

        SINGLE_LINE_COMMENT,
        MULTI_LINE_COMMENT
    }
}
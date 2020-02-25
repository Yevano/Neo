using Neo.AST;
using Neo.AST.Declarations;
using Neo.AST.Expressions;
using Neo.AST.Expressions.Atoms;
using Neo.AST.Statements;
using Neo.Frontend.Lexer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Neo.Frontend.Parser {
    public sealed class Parser {
        private readonly string name;
        private readonly List<Token> tokens;
        private int index;

        public Parser(string name, List<Token> tokens) {
            this.name = name;
            this.tokens = tokens.Where(t => t.Type != TokenType.SINGLE_LINE_COMMENT &&
                                            t.Type != TokenType.MULTI_LINE_COMMENT &&
                                            t.Type != TokenType.SHEBANG).ToList();
        }

        private void Next() => index++;

        private void Prev() => index--;

        private Token Current() {
            if (!More()) {
                throw new ParseException($"No more tokens in {name}");
            }

            return tokens[index];
        }

        private SourcePosition Position(int offset = 0) {
            return tokens[index + offset].Position;
        }

        private bool More() => index < tokens.Count;

        private bool Accept(params TokenType[] types) {
            if (!More()) return false;
            return types.Contains(Current().Type);
        }

        private void Unexpected() {
            throw new ParseException($"Unexpected token: {name}:{Current()}");
        }

        private void Expect(params TokenType[] types) {
            if (!Accept(types)) {
                var sb = new StringBuilder();

                foreach (var type in types) {
                    sb.Append(type);
                    sb.Append(" ");
                }

                throw new ParseException($"Unexpected token: {name}:{Current()} Possible types: {sb.ToString()}");
            }
        }

        private void ExpectNot(params TokenType[] types) {
            if (Accept(types)) {
                Unexpected();
            }
        }

        private UnaryOP ParseUnaryOP() {
            Expect(TokenType.BIT_NOT, TokenType.NOT, TokenType.LENGTH, TokenType.SUB, TokenType.UNPACK, TokenType.FROZEN, TokenType.INC, TokenType.DEC);

            if (Accept(TokenType.BIT_NOT)) {
                return UnaryOP.BIT_NOT;
            } else if (Accept(TokenType.NOT)) {
                return UnaryOP.NOT;
            } else if (Accept(TokenType.LENGTH)) {
                return UnaryOP.LENGTH;
            } else if (Accept(TokenType.SUB)) {
                return UnaryOP.NEG;
            } else if (Accept(TokenType.UNPACK)) {
                return UnaryOP.UNPACK;
            } else if (Accept(TokenType.FROZEN)) {
                return UnaryOP.FROZEN;
            } else if(Accept(TokenType.INC)) {
                return UnaryOP.PREFIX_INCREMENT;
            } else if(Accept(TokenType.DEC)) {
                return UnaryOP.PREFIX_DECREMENT;
            }

            throw new ParseException($"Unexpected token: {name}:{Current()}");
        }

        private BinaryOP ParseBinaryOP() {
            Expect(TokenType.ADD, TokenType.SUB, TokenType.MUL, TokenType.DIV, TokenType.POW, TokenType.MOD, TokenType.LSH, TokenType.RSH,
                TokenType.BIT_NOT, TokenType.BIT_AND, TokenType.BIT_OR, TokenType.BIT_XOR, TokenType.AND, TokenType.OR, TokenType.CONCAT,
                TokenType.EQ, TokenType.NE, TokenType.DEEP_EQ, TokenType.DEEP_NE, TokenType.LT, TokenType.GT, TokenType.LTE, TokenType.GTE);

            if (Accept(TokenType.ADD)) {
                return BinaryOP.ADD;
            } else if (Accept(TokenType.SUB)) {
                return BinaryOP.SUB;
            } else if (Accept(TokenType.MUL)) {
                return BinaryOP.MUL;
            } else if (Accept(TokenType.DIV)) {
                return BinaryOP.DIV;
            } else if (Accept(TokenType.POW)) {
                return BinaryOP.POW;
            } else if (Accept(TokenType.MOD)) {
                return BinaryOP.MOD;
            } else if (Accept(TokenType.LSH)) {
                return BinaryOP.LSH;
            } else if (Accept(TokenType.RSH)) {
                return BinaryOP.RSH;
            } else if (Accept(TokenType.BIT_NOT)) {
                return BinaryOP.BIT_NOT;
            } else if (Accept(TokenType.BIT_AND)) {
                return BinaryOP.BIT_AND;
            } else if (Accept(TokenType.BIT_OR)) {
                return BinaryOP.BIT_OR;
            } else if (Accept(TokenType.BIT_XOR)) {
                return BinaryOP.BIT_XOR;
            } else if (Accept(TokenType.AND)) {
                return BinaryOP.AND;
            } else if (Accept(TokenType.OR)) {
                return BinaryOP.OR;
            } else if (Accept(TokenType.CONCAT)) {
                return BinaryOP.CONCAT;
            } else if (Accept(TokenType.EQ)) {
                return BinaryOP.EQ;
            } else if (Accept(TokenType.NE)) {
                return BinaryOP.NE;
            } else if (Accept(TokenType.DEEP_EQ)) {
                return BinaryOP.DEEP_EQ;
            } else if (Accept(TokenType.DEEP_NE)) {
                return BinaryOP.DEEP_NE;
            } else if (Accept(TokenType.LT)) {
                return BinaryOP.LT;
            } else if (Accept(TokenType.GT)) {
                return BinaryOP.GT;
            } else if (Accept(TokenType.LTE)) {
                return BinaryOP.LTE;
            } else if (Accept(TokenType.GTE)) {
                return BinaryOP.GTE;
            }

            throw new ParseException($"Unexpected token: {name}:{Current()}");
        }

        private AssignOP ParseAssignOP() {
            Expect(TokenType.ASSIGN, TokenType.ADD_ASSIGN, TokenType.SUB_ASSIGN, TokenType.MUL_ASSIGN, TokenType.DIV_ASSIGN, TokenType.POW_ASSIGN, TokenType.MOD_ASSIGN, TokenType.LSH_ASSIGN, TokenType.RSH_ASSIGN, TokenType.BIT_NOT_ASSIGN, TokenType.BIT_AND_ASSIGN, TokenType.BIT_OR_ASSIGN, TokenType.BIT_XOR_ASSIGN, TokenType.AND_ASSIGN, TokenType.OR_ASSIGN, TokenType.CONCAT_ASSIGN);

            if (Accept(TokenType.ASSIGN)) {
                return AssignOP.NORMAL;
            } else if (Accept(TokenType.ADD_ASSIGN)) {
                return AssignOP.ADD;
            } else if (Accept(TokenType.SUB_ASSIGN)) {
                return AssignOP.SUB;
            } else if (Accept(TokenType.MUL_ASSIGN)) {
                return AssignOP.MUL;
            } else if (Accept(TokenType.DIV_ASSIGN)) {
                return AssignOP.DIV;
            } else if (Accept(TokenType.POW_ASSIGN)) {
                return AssignOP.POW;
            } else if (Accept(TokenType.MOD_ASSIGN)) {
                return AssignOP.MOD;
            } else if (Accept(TokenType.LSH_ASSIGN)) {
                return AssignOP.LSH;
            } else if (Accept(TokenType.RSH_ASSIGN)) {
                return AssignOP.RSH;
            } else if (Accept(TokenType.BIT_NOT_ASSIGN)) {
                return AssignOP.BIT_NOT;
            } else if (Accept(TokenType.BIT_AND_ASSIGN)) {
                return AssignOP.BIT_AND;
            } else if (Accept(TokenType.BIT_OR_ASSIGN)) {
                return AssignOP.BIT_OR;
            } else if (Accept(TokenType.BIT_XOR_ASSIGN)) {
                return AssignOP.BIT_XOR;
            } else if (Accept(TokenType.AND_ASSIGN)) {
                return AssignOP.AND;
            } else if (Accept(TokenType.OR_ASSIGN)) {
                return AssignOP.OR;
            } else if (Accept(TokenType.CONCAT_ASSIGN)) {
                return AssignOP.CONCAT;
            }

            throw new ParseException($"Unexpected token: {name}:{Current()}");
        }

        private IdentNode ParseIdent() {
            Expect(TokenType.IDENT);
            var ident = new IdentNode(Position(), Current().Text);
            Next();
            return ident;
        }

        private StringNode ParseString() {
            Expect(TokenType.STRING, TokenType.HERE_STRING);
            var str = new StringNode(Position(), Current().Text);
            Next();
            return str;
        }

        private ExpressionNode ParseInterpolatedString() {
            Expect(TokenType.INTERPOLATED_STRING);
            var pos = Position();
            var str = Current().Text;
            Next();

            var parts = new List<ExpressionNode>();

            bool inExpr = false;
            int level = 0;
            var start = 0;

            int i = 0;

            char peek() => str[i];
            string current() => str.Substring(start, i - start - 1);

            bool accept(char c) {
                if(peek() == c) {
                    i++;
                    return true;
                }
                return false;
            }

            void addString() {
                var cur = current();
                parts.Add(new StringNode(new SourcePosition(pos.Line, pos.Column + i - cur.Length + 1), cur));
            }

            void addExpression() {
                var cur = current();
                var lexer = new Lexer.Lexer(cur, new SourcePosition(pos.Line - 1, 0 /* @TODO wrong column number */));
                var parser = new Parser("<interpolated string expression>", lexer.Tokenize());
                parts.Add(parser.ParseExpression());
            }

            while(i < str.Length) {
                if(inExpr) {
                    if(accept('{')) {
                        level++;
                    } else if(accept('}')) {
                        level--;
                    }

                    if(level == 0) {
                        addExpression();
                        inExpr = false;
                        start = i;
                    } else {
                        i++;
                    }
                } else {
                    if(accept('{')) {
                        addString();
                        inExpr = true;
                        level = 1;
                        start = i;
                    } else {
                        i++;
                    }
                }
            }

            if(inExpr) throw new ParseException("String interpolation expression not ended");
            if(start != i) {
                i++;
                addString();
            }
            
            if(parts.Count == 1) {
                return parts[0];
            } else {
                var ret = new BinaryNode(pos /* @TODO: wrong column number */, BinaryOP.CONCAT, parts[0], parts[1]);

                for(var j = 2; j < parts.Count; j++) {
                    ret = new BinaryNode(pos /* @TODO: wrong column number */, BinaryOP.CONCAT, ret, parts[j]);
                }

                return ret;
            }
        }

        private BoolNode ParseBool() {
            Expect(TokenType.TRUE, TokenType.FALSE);
            var b = new BoolNode(Position(), Current().Type == TokenType.TRUE);
            Next();
            return b;
        }

        private IntNode ParseInt() {
            Expect(TokenType.INTEGER);
            var pos = Position();
            var str = Current().Text;
            
            var @base = 10;
            if(str.StartsWith("0x")) {
                str = str.Substring(2);
                @base = 16;
            } else if(str.StartsWith("0b")) {
                str = str.Substring(2);
                @base = 2;
            }

            Next();
            return new IntNode(pos, Convert.ToInt32(str, @base));
        }

        private FloatNode ParseFloat() {
            Expect(TokenType.FLOAT);
            var f = new FloatNode(Position(), double.Parse(Current().Text));
            Next();
            return f;
        }

        private NumberNode ParseNumber() {
            Expect(TokenType.FLOAT, TokenType.INTEGER);
            if (Accept(TokenType.FLOAT)) {
                return ParseFloat();
            } else {
                return ParseInt();
            }
        }

        private ExpressionNode ParseBinary(Func<ExpressionNode> next, params TokenType[] types) {
            var ret = next();

            while (More() && Accept(types)) {
                var pos = Position();
                var op = ParseBinaryOP();
                Next();
                ret = new BinaryNode(pos, op, ret, next());
            }

            return ret;
        }

        private ArrayNode ParseArray() {
            Expect(TokenType.LBRACK);
            Next();

            var array = new ArrayNode(Position(-1));

            while (More() && !Accept(TokenType.RBRACK)) {
                array.AddDefaultElement(ParseExpression());

                if (Accept(TokenType.COMMA)) {
                    Next();
                    ExpectNot(TokenType.RBRACK);
                } else {
                    Expect(TokenType.RBRACK);
                }
            }

            Expect(TokenType.RBRACK);
            Next();

            return array;
        }

        private ObjectNode ParseObject() {
            Expect(TokenType.LBRACE);
            Next();

            var obj = new ObjectNode(Position(-1));

            while (More() && !Accept(TokenType.RBRACE)) {
                Expect(TokenType.IDENT, TokenType.LBRACK);

                ExpressionNode key;
                if (Accept(TokenType.IDENT)) {
                    key = new StringNode(Position(), ParseIdent().Value);
                } else {
                    Expect(TokenType.LBRACK);
                    Next();
                    key = ParseExpression();
                    Expect(TokenType.RBRACK);
                    Next();
                }

                Expect(TokenType.ASSIGN);
                Next();

                obj.AddDefaultElement(key, ParseExpression());

                if (Accept(TokenType.COMMA)) {
                    ExpectNot(TokenType.RBRACE);
                    Next();
                } else {
                    Expect(TokenType.RBRACE);
                }
            }

            Expect(TokenType.RBRACE);
            Next();

            return obj;
        }

        private LambdaNode ParseLambda() {
            Expect(TokenType.PROC);
            Next();

            Expect(TokenType.LPAREN);
            Next();

            var lambda = new LambdaNode(Position(-2));

            while (More() && !Accept(TokenType.RPAREN)) {
                Expect(TokenType.FROZEN, TokenType.IDENT, TokenType.VARARGS);
                if (Accept(TokenType.FROZEN, TokenType.IDENT)) {
                    bool frozen = false;

                    while (Accept(TokenType.FROZEN)) {
                        frozen = true;
                        Next();
                    }

                    lambda.Parameters.Add(new ParameterNode(Position(), ParseIdent(), frozen));
                } else if (Accept(TokenType.VARARGS)) {
                    lambda.Varargs = true;
                    Next();
                    break;
                } else {
                    Unexpected();
                }

                if (Accept(TokenType.COMMA)) {
                    ExpectNot(TokenType.RPAREN);
                    Next();
                } else {
                    Expect(TokenType.RPAREN);
                }
            }

            Expect(TokenType.RPAREN);
            Next();

            Expect(TokenType.FAT_ARROW, TokenType.LBRACE);
            if (Accept(TokenType.FAT_ARROW)) {
                Next();

                lambda.Statements.Add(new ReturnNode(Position(), ParseExpression()));
            } else if (Accept(TokenType.LBRACE)) {
                Next();

                while (More() && !Accept(TokenType.RBRACE)) {
                    lambda.Statements.Add(ParseStatement());
                }

                Expect(TokenType.RBRACE);
                Next();
            } else {
                Unexpected();
            }

            return lambda;
        }

        private EnumNode ParseEnum(bool named = false) {
            var pos = Position();

            Expect(TokenType.ENUM);
            Next();

            if (named) {
                Expect(TokenType.IDENT);
                Next();
            }

            var e = new EnumNode(pos);

            Expect(TokenType.LBRACE);
            Next();

            while (More() && !Accept(TokenType.RBRACE)) {
                Expect(TokenType.IDENT);
                e.AddElement(Current().Text);
                Next();

                Expect(TokenType.COMMA, TokenType.RBRACE);
                if (Accept(TokenType.COMMA)) {
                    Next();
                    ExpectNot(TokenType.RBRACE);
                }
            }

            Expect(TokenType.RBRACE);
            Next();

            return e;
        }

        private ExpressionNode ParseAtom() {
            Expect(TokenType.TRUE, TokenType.FALSE, TokenType.NIL, TokenType.INTEGER, TokenType.FLOAT, TokenType.STRING, TokenType.HERE_STRING, TokenType.INTERPOLATED_STRING, 
                TokenType.LPAREN, TokenType.IDENT, TokenType.LBRACK, TokenType.LBRACE, TokenType.PROC, TokenType.VARARGS, TokenType.ENUM);

            if (Accept(TokenType.TRUE, TokenType.FALSE)) {
                return ParseBool();
            } else if (Accept(TokenType.NIL)) {
                Next();
                return new NilNode(Position(-1));
            } else if (Accept(TokenType.INTEGER, TokenType.FLOAT)) {
                return ParseNumber();
            } else if (Accept(TokenType.STRING, TokenType.HERE_STRING)) {
                return ParseString();
            } else if (Accept(TokenType.INTERPOLATED_STRING)) {
                return ParseInterpolatedString();
            } else if (Accept(TokenType.LPAREN)) {
                Next();
                var value = new ParenNode(Position(-1), ParseExpression());
                Expect(TokenType.RPAREN);
                Next();
                return value;
            } else if (Accept(TokenType.IDENT)) {
                return ParseIdent();
            } else if (Accept(TokenType.LBRACK)) {
                return ParseArray();
            } else if (Accept(TokenType.LBRACE)) {
                return ParseObject();
            } else if (Accept(TokenType.PROC)) {
                return ParseLambda();
            } else if (Accept(TokenType.VARARGS)) {
                Next();
                return new VarargsNode(Position(-1));
            } else if (Accept(TokenType.ENUM)) {
                return ParseEnum();
            }

            throw new Exception();
        }

        private ExpressionNode ParseBrackets() {
            var ret = ParseAtom();

            while (Accept(TokenType.LBRACK, TokenType.PERIOD, TokenType.LPAREN)) {
                var zero = new IntNode(Position(), 0);
                var length = new BinaryNode(Position(), BinaryOP.SUB, new UnaryNode(Position(), UnaryOP.LENGTH, ret), new IntNode(Position(), 1));

                var pos = Position();

                if (Accept(TokenType.LBRACK)) {
                    Next();

                    if (Accept(TokenType.COLON)) {
                        Next();

                        if (Accept(TokenType.RBRACK)) {
                            Next();
                            ret = new SliceNode(pos, ret, zero, length);
                        } else {
                            var end = ParseExpression();

                            Expect(TokenType.RBRACK);
                            Next();

                            ret = new SliceNode(pos, ret, zero, end);
                        }
                    } else {
                        var start = ParseExpression();

                        if (Accept(TokenType.COLON)) {
                            Next();

                            if (Accept(TokenType.RBRACK)) {
                                Next();
                                ret = new SliceNode(pos, ret, start, length);
                            } else {
                                var end = ParseExpression();

                                Expect(TokenType.RBRACK);
                                Next();

                                ret = new SliceNode(pos, ret, start, end);
                            }
                        } else {
                            Expect(TokenType.RBRACK);
                            Next();

                            ret = new IndexNode(pos, ret, start);
                        }
                    }
                } else if (Accept(TokenType.PERIOD)) {
                    Next();
                    ret = new IndexNode(Position(-1), ret, new StringNode(Position(), ParseIdent().Value));
                } else if (Accept(TokenType.LPAREN)) {
                    Next();

                    var call = new CallNode(pos, ret);

                    while (More() && !Accept(TokenType.RPAREN)) {
                        call.Arguments.Add(ParseExpression());

                        if (Accept(TokenType.COMMA)) {
                            ExpectNot(TokenType.RPAREN);
                            Next();
                        } else {
                            Expect(TokenType.RPAREN);
                        }
                    }

                    Expect(TokenType.RPAREN);
                    Next();

                    ret = call;
                } else {
                    Unexpected();
                }
            }

            return ret;
        }

        private ExpressionNode ParsePower() {
            return ParseBinary(ParseBrackets, TokenType.POW);
        }

        private ExpressionNode ParsePostfixIncrementOrDecrement() {
            var ret = ParsePower();

            while(More() && Accept(TokenType.INC, TokenType.DEC)) {
                UnaryOP op;
                if(Accept(TokenType.INC)) {
                    op = UnaryOP.POSTFIX_INCREMENT;
                } else {
                    op = UnaryOP.POSTFIX_DECREMENT;
                }

                ret = new UnaryNode(Position(), op, ret);
                Next();
            }

            return ret;
        }

        private ExpressionNode ParseUnary() {
            if (Accept(TokenType.BIT_NOT, TokenType.NOT, TokenType.LENGTH, TokenType.SUB, TokenType.UNPACK, TokenType.FROZEN, TokenType.INC, TokenType.DEC)) {
                var ops = new List<UnaryOP>();
                while (Accept(TokenType.BIT_NOT, TokenType.NOT, TokenType.LENGTH, TokenType.SUB, TokenType.UNPACK, TokenType.FROZEN, TokenType.INC, TokenType.DEC)) {
                    ops.Add(ParseUnaryOP());
                    Next();
                }

                var ret = ParsePostfixIncrementOrDecrement();

                for (var i = ops.Count - 1; i >= 0; i--) {
                    ret = new UnaryNode(Position(), ops[i], ret);
                }

                return ret;
            } else {
                return ParsePostfixIncrementOrDecrement();
            }
        }

        private ExpressionNode ParseBinaryMul() => ParseBinary(ParseUnary, TokenType.MUL, TokenType.DIV, TokenType.MOD);

        private ExpressionNode ParseBinaryAdd() => ParseBinary(ParseBinaryMul, TokenType.ADD, TokenType.SUB);

        private ExpressionNode ParseBinaryShift() => ParseBinary(ParseBinaryAdd, TokenType.LSH, TokenType.RSH);

        private ExpressionNode ParseBinaryComparison() => ParseBinary(ParseBinaryShift, TokenType.LT, TokenType.GT, TokenType.LTE, TokenType.GTE);

        private ExpressionNode ParseBinaryEquality() => ParseBinary(ParseBinaryComparison, TokenType.EQ, TokenType.NE, TokenType.DEEP_EQ, TokenType.DEEP_NE);

        private ExpressionNode ParseBinaryAnd() => ParseBinary(ParseBinaryEquality, TokenType.BIT_AND);

        private ExpressionNode ParseBinaryXor() => ParseBinary(ParseBinaryAnd, TokenType.BIT_XOR);

        private ExpressionNode ParseBinaryOr() => ParseBinary(ParseBinaryXor, TokenType.BIT_OR);

        private ExpressionNode ParseBinaryLogicalAnd() => ParseBinary(ParseBinaryOr, TokenType.AND);

        private ExpressionNode ParseBinaryLogicalOr() => ParseBinary(ParseBinaryLogicalAnd, TokenType.OR);

        private ExpressionNode ParseConcat() => ParseBinary(ParseBinaryLogicalOr, TokenType.CONCAT);

        private ExpressionNode ParseExpression() {
            var ret = ParseConcat();

            while (More() && Accept(TokenType.QUESTION_MARK)) {
                var pos = Position();

                Next();

                var @true = ParseExpression();

                Expect(TokenType.COLON);
                Next();

                var @false = ParseExpression();

                ret = new TernaryNode(pos, ret, @true, @false);
            }

            return ret;
        }

        private VarNode ParseVar(bool exported) {
            var pos = Position();
            bool final = false;

            Expect(TokenType.FINAL, TokenType.VAR);
            if (Accept(TokenType.FINAL)) {
                Next();
                final = true;
            } else if (Accept(TokenType.VAR)) {
                Next();
                final = false;
            } else {
                throw new Exception();
            }

            var name = ParseIdent();
            ExpressionNode defaultValue = null;

            if (Accept(TokenType.ASSIGN)) {
                Next();
                defaultValue = ParseExpression();
            }

            Expect(TokenType.SEMICOLON);
            Next();

            return new VarNode(pos, name, defaultValue, exported, final);
        }

        private IfNode ParseIf() {
            var pos = Position();

            Expect(TokenType.IF);
            Next();

            var condition = ParseExpression();
            var @true = ParseStatement();
            StatementNode @false = null;

            if (Accept(TokenType.ELSE)) {
                Next();
                @false = ParseStatement();
            }

            return new IfNode(pos, condition, @true, @false);
        }

        private ForNode ParseFor() {
            var pos = Position();

            Expect(TokenType.FOR);
            Next();

            var key = ParseIdent();

            Expect(TokenType.IN, TokenType.COMMA);
            if (Accept(TokenType.COMMA)) {
                Next();

                var value = ParseIdent();

                Expect(TokenType.IN);
                Next();

                var from = ParseExpression();
                var code = ParseStatement();

                return new ForKeyValueIteratorNode(pos, key, value, from, code);
            } else if (Accept(TokenType.IN)) {
                Next();

                var from = ParseExpression();

                if (Accept(TokenType.TO)) {
                    Next();

                    var end = ParseExpression();
                    ExpressionNode by = null;

                    if (Accept(TokenType.BY)) {
                        Next();
                        by = ParseExpression();
                    }

                    var code = ParseStatement();

                    return new ForRangeNode(pos, key, from, end, by, code);
                } else {
                    return new ForIteratorNode(pos, key, from, ParseStatement());
                }
            } else {
                throw new ParseException($"Unexpected token: {name}:{Current()}");
            }
        }

        private WhileNode ParseWhile() {
            var pos = Position();

            Expect(TokenType.WHILE);
            Next();

            var condition = ParseExpression();
            var code = ParseStatement();

            return new WhileNode(pos, condition, code);
        }

        private DoNode ParseDo() {
            var pos = Position();

            Expect(TokenType.DO);
            Next();

            var code = ParseStatement();

            Expect(TokenType.WHILE);
            Next();

            var condition = ParseExpression();

            Expect(TokenType.SEMICOLON);
            Next();

            return new DoNode(pos, condition, code);
        }

        private ReturnNode ParseReturn() {
            var pos = Position();

            Expect(TokenType.RETURN);
            Next();

            if (Accept(TokenType.SEMICOLON)) {
                Next();
                return new ReturnNode(pos, new NilNode(Position(-1)));
            } else {
                var value = ParseExpression();

                Expect(TokenType.SEMICOLON);
                Next();

                return new ReturnNode(pos, value);
            }
        }

        private ImportNode ParseImport() {
            Expect(TokenType.IMPORT);
            Next();

            var import = new ImportNode(Position(-1), ParseString());

            Expect(TokenType.AS, TokenType.SEMICOLON);
            if (Accept(TokenType.AS)) {
                Next();
                import.Alias = ParseIdent();
            }

            Expect(TokenType.SEMICOLON);
            Next();

            return import;
        }

        private BlockNode ParseBlock() {
            Expect(TokenType.LBRACE);
            Next();

            var block = new BlockNode(Position(-1));

            while (More() && !Accept(TokenType.RBRACE)) {
                block.AddStatement(ParseStatement());
            }

            Expect(TokenType.RBRACE);
            Next();

            return block;
        }

        private ProcNode ParseProc(bool exported) {
            Expect(TokenType.PROC);
            Next();

            var proc = new ProcNode(Position(-1), ParseIdent(), exported);

            Expect(TokenType.LPAREN);
            Next();

            while (More() && !Accept(TokenType.RPAREN)) {
                Expect(TokenType.FROZEN, TokenType.IDENT, TokenType.VARARGS);

                var pos = Position();
                if (Accept(TokenType.FROZEN, TokenType.IDENT)) {
                    bool frozen = false;

                    while (Accept(TokenType.FROZEN)) {
                        frozen = true;
                        Next();
                    }

                    proc.Parameters.Add(new ParameterNode(pos, ParseIdent(), frozen));
                } else if (Accept(TokenType.VARARGS)) {
                    proc.Varargs = true;
                    Next();
                    break;
                } else {
                    Unexpected();
                }

                if (Accept(TokenType.COMMA)) {
                    ExpectNot(TokenType.RPAREN);
                    Next();
                } else {
                    Expect(TokenType.RPAREN);
                }
            }

            Expect(TokenType.RPAREN);
            Next();

            Expect(TokenType.FAT_ARROW, TokenType.LBRACE);
            if (Accept(TokenType.FAT_ARROW)) {
                Next();

                proc.Statements.Add(new ReturnNode(Position(), ParseExpression()));
            } else if (Accept(TokenType.LBRACE)) {
                Next();

                while (More() && !Accept(TokenType.RBRACE)) {
                    proc.Statements.Add(ParseStatement());
                }

                Expect(TokenType.RBRACE);
                Next();
            } else {
                Unexpected();
            }

            return proc;
        }

        private TryCatchNode ParseTryCatch() {
            var pos = Position();

            Expect(TokenType.TRY);
            Next();

            var @try = ParseStatement();

            Expect(TokenType.CATCH);
            Next();

            var error = ParseIdent();
            var @catch = ParseStatement();

            return new TryCatchNode(pos, @try, @catch, error);
        }

        private ThrowNode ParseThrow() {
            var pos = Position();

            Expect(TokenType.THROW);
            Next();

            var error = ParseExpression();

            Expect(TokenType.SEMICOLON);
            Next();

            return new ThrowNode(pos, error);
        }

        private EnumDeclarationNode ParseEnumDeclaration(bool exported) {
            var pos = Position();

            Expect(TokenType.ENUM);
            Next();

            var name = ParseIdent().Value;

            Prev();
            Prev();

            var e = ParseEnum(true);

            return new EnumDeclarationNode(pos, e, name, exported);
        }

        private StatementNode ParseStatement() {
            Expect(TokenType.EXPORTED, TokenType.PROC, TokenType.FINAL, TokenType.VAR, TokenType.IF, TokenType.BREAK, TokenType.CONTINUE, TokenType.FOR,
                TokenType.WHILE, TokenType.DO, TokenType.DEFER, TokenType.RETURN, TokenType.IDENT, TokenType.LPAREN, TokenType.LBRACE,
                TokenType.TRY, TokenType.THROW, TokenType.ENUM, TokenType.INC, TokenType.DEC);

            if (Accept(TokenType.PROC)) {
                return ParseProc(false);
            } else if (Accept(TokenType.FINAL, TokenType.VAR)) {
                return ParseVar(false);
            } else if (Accept(TokenType.IF)) {
                return ParseIf();
            } else if (Accept(TokenType.BREAK)) {
            	var pos = Position();
                Next();
                Expect(TokenType.SEMICOLON);
                Next();
                return new BreakNode(pos);
            } else if (Accept(TokenType.CONTINUE)) {
            	var pos = Position();
                Next();
                Expect(TokenType.SEMICOLON);
                Next();
                return new ContinueNode(pos);
            } else if (Accept(TokenType.FOR)) {
                return ParseFor();
            } else if (Accept(TokenType.WHILE)) {
                return ParseWhile();
            } else if (Accept(TokenType.DO)) {
                return ParseDo();
            } else if (Accept(TokenType.DEFER)) {
                var pos = Position();
                Next();
                return new DeferNode(pos, ParseStatement());
            } else if (Accept(TokenType.RETURN)) {
                return ParseReturn();
            } else if (Accept(TokenType.IDENT, TokenType.LPAREN, TokenType.INC, TokenType.DEC)) {
                var pos = Position();
                var left = ParseExpression();

                if (left is CallNode || (left is UnaryNode u && (u.OP == UnaryOP.PREFIX_INCREMENT || u.OP == UnaryOP.PREFIX_DECREMENT || u.OP == UnaryOP.POSTFIX_INCREMENT || u.OP == UnaryOP.POSTFIX_DECREMENT))) {
                    var stmt = new ExpressionStatementNode(pos, left);
                    Expect(TokenType.SEMICOLON);
                    Next();
                    return stmt;
                } else {
                    if (Accept(TokenType.ASSIGN, TokenType.ADD_ASSIGN, TokenType.SUB_ASSIGN, TokenType.MUL_ASSIGN, TokenType.DIV_ASSIGN, TokenType.POW_ASSIGN, TokenType.MOD_ASSIGN, TokenType.LSH_ASSIGN, TokenType.RSH_ASSIGN, TokenType.BIT_NOT_ASSIGN, TokenType.BIT_AND_ASSIGN, TokenType.BIT_OR_ASSIGN, TokenType.BIT_XOR_ASSIGN, TokenType.AND_ASSIGN, TokenType.OR_ASSIGN, TokenType.CONCAT_ASSIGN)) {
                        if (!(left is IdentNode || left is IndexNode)) {
                            Unexpected();
                        }

                        var pos2 = Position();

                        var op = ParseAssignOP();
                        Next();

                        var right = ParseExpression();

                        Expect(TokenType.SEMICOLON);
                        Next();

                        return new AssignNode(pos, op, left, right);
                    } else {
                        Unexpected();
                    }
                }
            } else if (Accept(TokenType.LBRACE)) {
                return ParseBlock();
            } else if (Accept(TokenType.TRY)) {
                return ParseTryCatch();
            } else if (Accept(TokenType.THROW)) {
                return ParseThrow();
            } else if (Accept(TokenType.ENUM)) {
                return ParseEnumDeclaration(false);
            }

            throw new Exception();
        }

        public ChunkNode ParseChunk() {
            var chunk = new ChunkNode(Position(), name);

            while (More()) {
                Expect(TokenType.IMPORT, TokenType.EXPORTED, TokenType.PROC, TokenType.FINAL, TokenType.VAR, TokenType.ENUM);
                if (Accept(TokenType.IMPORT)) {
                    chunk.Imports.Add(ParseImport());
                } else if (Accept(TokenType.EXPORTED)) {
                    Next();
                    if (Accept(TokenType.PROC)) {
                        chunk.Procedures.Add(ParseProc(true));
                    } else if (Accept(TokenType.FINAL, TokenType.VAR)) {
                        chunk.Variables.Add(ParseVar(true));
                    } else if (Accept(TokenType.ENUM)) {
                        chunk.Enums.Add(ParseEnumDeclaration(true));
                    } else {
                        Unexpected();
                    }
                } else if (Accept(TokenType.PROC)) {
                    chunk.Procedures.Add(ParseProc(false));
                } else if (Accept(TokenType.FINAL, TokenType.VAR)) {
                    chunk.Variables.Add(ParseVar(false));
                } else if (Accept(TokenType.ENUM)) {
                    chunk.Enums.Add(ParseEnumDeclaration(false));
                } else {
                    throw new Exception();
                }
            }

            return chunk;
        }
    }
}
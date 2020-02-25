using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Neo.Frontend.Lexer {
    public sealed class Lexer {
        private string source;
        private int start;
        private int pos;
        private List<Token> tokens;
        private int line;
        private int column;
        private SourcePosition offset;

        public Lexer(string source) : this(source, SourcePosition.NIL) {
        }

        public Lexer(string source, SourcePosition offset) {
            this.source = source;
            tokens = new List<Token>();

            line = 1;
            column = 1;

            this.offset = offset;
        }

        private string Current() {
            return source.Substring(start, pos - start);
        }

        private void Emit(TokenType type) {
            tokens.Add(new Token(type, Current(), new SourcePosition(line + offset.Line, column - (pos - start) + offset.Column)));
            start = pos;
        }

        private char Next() {
            if (!More()) {
                return (char)0;
            }

            var c = source[pos];
            pos++;
            column++;
            return c;
        }

        private char Peek() {
            if (!More()) {
                return (char)0;
            }

            return source[pos];
        }

        private void Ignore() => start = pos;

        private bool Accept(string valid) {
            if (!More()) {
                return false;
            }

            if (valid.Contains(char.ToString(Peek()))) {
                Next();
                return true;
            }

            return false;
        }

        private void AcceptRun(string valid) {
            while (More() && valid.Contains(char.ToString(Peek()))) {
                Next();
            }
        }

        private bool AcceptSeq(string seq) {
            var chars = seq.ToCharArray();
            var savedPos = pos;
            var savedColumn = column;
            for (int i = 0; i < seq.Length; i++) {
                if (chars[i] != Peek()) {
                    pos = savedPos;
                    column = savedColumn;
                    return false;
                } else {
                    Next();
                }
            }
            return true;
        }

        private bool More() => pos < source.Length;

        private void SkipWhitespace() {
            var r = true;
            while(r) {
                var c = Peek();
                switch(c) {
                    case '\n':
                        line++;
                        column = 0;
                        Next();
                        break;
                    case '\t':
                        Next();
                        break;
                    case ' ':
                        Next();
                        break;
                    default:
                        r = false;
                        break;
                }
            }
            Ignore();
        }

        private string ParseString(bool interpolation) {
            var sb = new StringBuilder();

            bool inExpr = false;
            int level = 0;

            while (More()) {
                if(!inExpr && Peek() == '"') break;
                
                if(inExpr) {
                    if(Accept("{")) {
                        level++;
                        sb.Append("{");
                    } else if(Accept("}")) {
                        level--;
                        sb.Append("}");
                    }

                    if(level == 0) inExpr = false;
                    else sb.Append(Next());
                } else {
                    if(interpolation && Accept("{")) {
                        inExpr = true;
                        level = 1;
                        sb.Append("{");
                    } else if (Accept("\\")) {
                        if(interpolation && Accept("{")) {
                            sb.Append("{");
                        } else if (Accept("\\")) {
                            sb.Append("\\");
                        } else if (Accept("\"")) {
                            sb.Append("\"");
                        } else if (Accept("b")) {
                            sb.Append((char)8);
                        } else if (Accept("f")) {
                            sb.Append((char)12);
                        } else if (Accept("n")) {
                            sb.Append('\n');
                        } else if (Accept("r")) {
                            sb.Append((char)13);
                        } else if (Accept("t")) {
                            sb.Append((char)9);
                        } else if (Accept("u")) {
                            var cb = new StringBuilder();
                            for (var j = 0; j < 4; j++) {
                                cb.Append(Peek());
                                Next();
                            }

                            if (int.TryParse(cb.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int i)) {
                                sb.Append((char)i);
                            } else {
                                throw new LexException($"Invalid character: \\u{cb.ToString()}");
                            }
                        } else {
                            throw new LexException($"Invalid escape character '{Peek()}'");
                        }
                    } else {
                        sb.Append(Next());
                    }    
                }
            }

            if(interpolation && inExpr) throw new Exception("Interpolated string expression not closed");

            Next();
            Ignore();

            return sb.ToString();
        }

        public List<Token> Tokenize() {
            var running = true;
            while (running) {
                SkipWhitespace();

                var c = Next();
                switch (c) {
                    case (char)0:
                        running = false;
                        break;
                    case '(':
                        Emit(TokenType.LPAREN);
                        break;
                    case ')':
                        Emit(TokenType.RPAREN);
                        break;
                    case '{':
                        Emit(TokenType.LBRACE);
                        break;
                    case '}':
                        Emit(TokenType.RBRACE);
                        break;
                    case '[':
                        Emit(TokenType.LBRACK);
                        break;
                    case ']':
                        Emit(TokenType.RBRACK);
                        break;
                    case ';':
                        Emit(TokenType.SEMICOLON);
                        break;
                    case ':':
                        Emit(TokenType.COLON);
                        break;
                    case ',':
                        Emit(TokenType.COMMA);
                        break;
                    case '.':
                        if (Accept(".")) {
                            if (Accept(".")) {
                                Emit(TokenType.VARARGS);
                            } else if (Accept("=")) {
                                Emit(TokenType.CONCAT_ASSIGN);
                            } else {
                                Emit(TokenType.CONCAT);
                            }
                        } else {
                            Emit(TokenType.PERIOD);
                        }
                        break;
                    case '#':
                        if (Accept("!")) {
                            while (More() && Peek() != '\n') {
                                Next();
                            }
                            Emit(TokenType.SHEBANG);
                        } else {
                            Emit(TokenType.LENGTH);
                        }
                        break;
                    case '?':
                        Emit(TokenType.QUESTION_MARK);
                        break;
                    case '+':
                        if (Accept("+")) {
                            Emit(TokenType.INC);
                        } else if (Accept("=")) {
                            Emit(TokenType.ADD_ASSIGN);
                        } else {
                            Emit(TokenType.ADD);
                        }
                        break;
                    case '-':
                        if (Accept("-")) {
                            Emit(TokenType.DEC);
                        } else if (Accept("=")) {
                            Emit(TokenType.SUB_ASSIGN);
                        } else {
                            Emit(TokenType.SUB);
                        }
                        break;
                    case '*':
                        if (Accept("*")) {
                            if (Accept("=")) {
                                Emit(TokenType.POW_ASSIGN);
                            } else {
                                Emit(TokenType.POW);
                            }
                        } else {
                            if (Accept("=")) {
                                Emit(TokenType.MUL_ASSIGN);
                            } else {
                                Emit(TokenType.MUL);
                            }
                        }
                        break;
                    case '/':
                        if (Accept("/")) {
                            while (More() && Peek() != '\n') {
                                Next();
                            }
                            Emit(TokenType.SINGLE_LINE_COMMENT);
                        } else if (Accept("*")) {
                            var depth = 1;

                            Next();
                            while (More() && depth > 0) {
                                if (AcceptSeq("/*")) {
                                    depth++;
                                } else if (AcceptSeq("*/")) {
                                    depth--;
                                } else if (Peek() == '\n') {
                                    line++;
                                    column = 1;
                                }

                                Next();
                            }

                            if (depth != 0) {
                                throw new LexException("Block comment not ended");
                            }

                            Emit(TokenType.MULTI_LINE_COMMENT);
                        } else if (Accept("=")) {
                            Emit(TokenType.DIV_ASSIGN);
                        } else {
                            Emit(TokenType.DIV);
                        }
                        break;
                    case '%':
                        if (Accept("=")) {
                            Emit(TokenType.MOD_ASSIGN);
                        } else {
                            Emit(TokenType.MOD);
                        }
                        break;
                    case '<':
                        if (Accept("<")) {
                            if (Accept("=")) {
                                Emit(TokenType.LSH_ASSIGN);
                            } else {
                                Emit(TokenType.LSH);
                            }
                        } else {
                            if (Accept("=")) {
                                Emit(TokenType.LTE);
                            } else {
                                Emit(TokenType.LT);
                            }
                        }
                        break;
                    case '>':
                        if (Accept(">")) {
                            if (Accept("=")) {
                                Emit(TokenType.RSH_ASSIGN);
                            } else {
                                Emit(TokenType.RSH);
                            }
                        } else {
                            if (Accept("=")) {
                                Emit(TokenType.GTE);
                            } else {
                                Emit(TokenType.GT);
                            }
                        }
                        break;
                    case '~':
                        if (Accept("=")) {
                            Emit(TokenType.BIT_NOT_ASSIGN);
                        } else {
                            Emit(TokenType.BIT_NOT);
                        }
                        break;
                    case '&':
                        if (Accept("&")) {
                            if (Accept("=")) {
                                Emit(TokenType.AND_ASSIGN);
                            } else {
                                Emit(TokenType.AND);
                            }
                        } else if (Accept("=")) {
                            Emit(TokenType.BIT_AND_ASSIGN);
                        } else {
                            Emit(TokenType.BIT_AND);
                        }
                        break;
                    case '|':
                        if (Accept("|")) {
                            if (Accept("=")) {
                                Emit(TokenType.OR_ASSIGN);
                            } else {
                                Emit(TokenType.OR);
                            }
                        } else if (Accept("=")) {
                            Emit(TokenType.BIT_OR_ASSIGN);
                        } else {
                            Emit(TokenType.BIT_OR);
                        }
                        break;
                    case '^':
                        Emit(TokenType.BIT_XOR);
                        break;
                    case '!':
                        if (Accept("=")) {
                            if (Accept("=")) {
                                Emit(TokenType.DEEP_NE);
                            } else {
                                Emit(TokenType.NE);
                            }
                        } else {
                            Emit(TokenType.NOT);
                        }
                        break;
                    case '=':
                        if (Accept("=")) {
                            if (Accept("=")) {
                                Emit(TokenType.DEEP_EQ);
                            } else {
                                Emit(TokenType.EQ);
                            }
                        } else if (Accept(">")) {
                            Emit(TokenType.FAT_ARROW);
                        } else {
                            Emit(TokenType.ASSIGN);
                        }
                        break;  
                    case '$':
                        if(Accept("\"")) {
                            var str = ParseString(true);
                            tokens.Add(new Token(TokenType.INTERPOLATED_STRING, str, new SourcePosition(line + offset.Line, column - str.Length + offset.Column)));
                            start = pos;
                        } else {
                            Emit(TokenType.UNPACK);
                        }
                        break;
                    case '"': {
                            var str = ParseString(false);
                            tokens.Add(new Token(TokenType.STRING, str, new SourcePosition(line + offset.Line, column - str.Length - 2 + offset.Column)));
                            start = pos;
                        }
                        break;
                    case '@':
                        if (Accept("\"")) {
                            var hsb = new StringBuilder();  

                            bool ended = false;
                            while (More()) {
                                if (AcceptSeq("\"@")) {
                                    ended = true;
                                    break;
                                } else {
                                    var c2 = Peek();

                                    if (c2 == '\n') {
                                        line++;
                                        column = 1;
                                    }

                                    hsb.Append(c2);
                                    Next();
                                }
                            }

                            if (!ended) {
                                throw new LexException("Here string never ends");
                            }

                            Ignore();

                            tokens.Add(new Token(TokenType.HERE_STRING, hsb.ToString(), new SourcePosition(line + 1 + offset.Line, column - (pos - start) + 1 + offset.Column)));
                            start = pos;
                        } else {
                            throw new LexException($"Unexpected character: '{c}'");
                        }
                        break;
                    default:
                        if (char.IsLetter(c) || c == '_') {
                            while (More() && (char.IsLetterOrDigit(Peek()) || Peek() == '_')) {
                                Next();
                            }

                            var ident = Current();
                            switch (ident) {
                                case "exported":
                                    Emit(TokenType.EXPORTED);
                                    break;
                                case "var":
                                    Emit(TokenType.VAR);
                                    break;
                                case "if":
                                    Emit(TokenType.IF);
                                    break;
                                case "else":
                                    Emit(TokenType.ELSE);
                                    break;
                                case "break":
                                    Emit(TokenType.BREAK);
                                    break;
                                case "continue":
                                    Emit(TokenType.CONTINUE);
                                    break;
                                case "for":
                                    Emit(TokenType.FOR);
                                    break;
                                case "in":
                                    Emit(TokenType.IN);
                                    break;
                                case "to":
                                    Emit(TokenType.TO);
                                    break;
                                case "by":
                                    Emit(TokenType.BY);
                                    break;
                                case "while":
                                    Emit(TokenType.WHILE);
                                    break;
                                case "do":
                                    Emit(TokenType.DO);
                                    break;
                                case "defer":
                                    Emit(TokenType.DEFER);
                                    break;
                                case "return":
                                    Emit(TokenType.RETURN);
                                    break;
                                case "proc":
                                    Emit(TokenType.PROC);
                                    break;
                                case "import":
                                    Emit(TokenType.IMPORT);
                                    break;
                                case "as":
                                    Emit(TokenType.AS);
                                    break;
                                case "try":
                                    Emit(TokenType.TRY);
                                    break;
                                case "catch":
                                    Emit(TokenType.CATCH);
                                    break;
                                case "throw":
                                    Emit(TokenType.THROW);
                                    break;
                                case "final":
                                    Emit(TokenType.FINAL);
                                    break;
                                case "frozen":
                                    Emit(TokenType.FROZEN);
                                    break;
                                case "enum":
                                    Emit(TokenType.ENUM);
                                    break;
                                case "true":
                                    Emit(TokenType.TRUE);
                                    break;
                                case "false":
                                    Emit(TokenType.FALSE);
                                    break;
                                case "nil":
                                    Emit(TokenType.NIL);
                                    break;
                                default:
                                    Emit(TokenType.IDENT);
                                    break;
                            }
                        } else if (char.IsDigit(c)) {
                            if(c == '0' && (Peek() == 'x' || Peek() == 'b')) {
                                if (Accept("x")) {
                                    AcceptRun("0123456789ABCDEFabcdef");
                                } else if(Accept("b")) {
                                    AcceptRun("01");
                                }

                                Emit(TokenType.INTEGER);        
                            } else {
                                AcceptRun("0123456789");
                                if (Accept(".")) {
                                    AcceptRun("0123456789");
                                    Emit(TokenType.FLOAT);
                                } else {
                                    Emit(TokenType.INTEGER);
                                }
                            }
                        } else {
                            throw new LexException($"Unexpected character '{c}'");
                        }
                        break;
                }
            }
            return tokens;
        }
    }
}
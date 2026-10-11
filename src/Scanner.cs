class Scanner {
    // source code to scan
    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType> {
        ["INITIALIZE"] = TokenType.INITIALIZE,
        ["SET"] = TokenType.SET,
        ["INPUT"] = TokenType.INPUT,
        ["OUTPUT"] = TokenType.OUTPUT,
        ["PRINT"] = TokenType.PRINT,

        ["IF"] = TokenType.IF,
        ["THEN"] = TokenType.THEN,
        ["ELSE"] = TokenType.ELSE,

        ["FOR"] = TokenType.FOR,
        ["EACH"] = TokenType.EACH,
        ["IN"] = TokenType.IN,
        ["TO"] = TokenType.TO,
        ["STEP"] = TokenType.STEP,

        ["WHILE"] = TokenType.WHILE,
        ["DO"] = TokenType.DO,

        ["REPEAT"] = TokenType.REPEAT,
        ["UNTIL"] = TokenType.UNTIL,

        ["CASE"] = TokenType.CASE,
        ["OF"] = TokenType.OF,
        ["DEFAULT"] = TokenType.DEFAULT,

        ["FUNCTION"] = TokenType.FUNCTION,
        ["PROCEDURE"] = TokenType.PROCEDURE,
        ["CALL"] = TokenType.CALL,
        ["RETURN"] = TokenType.RETURN,

        ["END"] = TokenType.END,

        ["TRUE"] = TokenType.TRUE,
        ["FALSE"] = TokenType.FALSE,
        ["NIL"] = TokenType.NIL,

        ["AND"] = TokenType.AND,
        ["OR"] = TokenType.OR,
        ["NOT"] = TokenType.NOT,
        ["XOR"] = TokenType.XOR,

        ["MOD"] = TokenType.MOD
        };
    private List<string> errorMessages = [];
    public IReadOnlyList<Token> Tokens => _tokens;
    public IReadOnlyList<string> ErrorMessages => errorMessages;
    public bool HasErrors => errorMessages.Count > 0;

    // marker references
    private int _current = 0;
    private int _start = 0;
    private int _line = 1;

    public Scanner(string source) => _source = source;
    
    // helper methods
    private bool IsAtEnd() => _current >= _source.Length;
    private char Advance() => _source[_current++];
    private char Peek(int offset = 0) => (IsAtEnd() || _current + offset >= _source.Length) ? '\0' : _source[_current + offset];
    
    private bool Match(char expected) {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;
        _current++;
        return true;
    }
    private void AddToken(TokenType type, object? literal = null) => _tokens.Add(new Token(type, _source[_start .. _current], literal, _line));
    private void FlagError(string message) {
        errorMessages.Add($"[Line {_line}] Lexical Error: {message}");
    }
    public void PrintTokens(int mode = 0){
        if (HasErrors) {
            foreach (string error in errorMessages) Console.Error.WriteLine(error);
            if (mode == 0) Environment.Exit(65);
        }
        foreach (Token token in _tokens) Console.WriteLine(token);
    }
    
    // core scanner methods
    public void ScanTokens() {
        while (!IsAtEnd()) {_start = _current; ScanToken();}
        _start = _source.Length;
        AddToken(TokenType.EOF);
    }

    private void ScanToken() {
        char c = Advance();
        switch (c) {
            // single character tokens
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case ',': AddToken(TokenType.COMMA); break;
            case ':': AddToken(TokenType.COLON); break;
            case '.': AddToken(TokenType.DOT); break;
            case '=': AddToken(TokenType.EQUAL); break;
            case '+': AddToken(TokenType.PLUS); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '/': AddToken(TokenType.SLASH); break;
            case '%': AddToken(TokenType.MOD); break;
            case '^': AddToken(TokenType.EXP); break;
            case '[': AddToken(TokenType.LEFT_BRACKET); break;
            case ']': AddToken(TokenType.RIGHT_BRACKET); break;

            // tokens that can be more than one character
            case '*': 
                if (Match('*') )        AddToken(TokenType.EXP);
                else                    AddToken(TokenType.STAR);
                break;
                
            case '<':
                if (Match('='))         AddToken(TokenType.LESSER_EQUAL);
                else if (Match('-'))    AddToken(TokenType.ASSIGN);
                else                    AddToken(TokenType.LESSER);
                break;

            case '>':
                if (Match('='))         AddToken(TokenType.GREATER_EQUAL);
                // skip comments starting with ">>"
                else if (Match('>'))    while (Peek() != '\n' && !IsAtEnd()) Advance();
                else                    AddToken(TokenType.GREATER);
                break;

            case '!':
                if (Match('='))         AddToken(TokenType.NOT_EQUAL);
                else                    AddToken(TokenType.NOT);
                break;

            case '&':
                if (Match('&'))         AddToken(TokenType.AND);
                else                    FlagError($"Unexpected character '{c}' found in scan block.");
                break;

            // ignore whitespace
            case ' ' or '\t' or '\r':   break;
            case '\n':                  _line++; break;

            case '|':
                if (Match('|'))         AddToken(TokenType.OR);
                else                    FlagError($"Unexpected character '{c}' found in scan block.");
                break;

            case '"':
                ScanString();
                break;

            default:
                if (char.IsLetter(c) || c == '_')       ScanKeyword();
                else if (char.IsDigit(c))               ScanNumber(); 
                else                                    FlagError($"Unexpected character '{c}' found in scan block.");
                break;
        }
    }

    private void ScanKeyword() {
        while ((char.IsLetterOrDigit(Peek()) || Peek() == '_') && !IsAtEnd()) Advance();
        string text = _source[_start .. _current];

        if (_keywords.TryGetValue(text, out TokenType type)) {
            switch (type) {
                case TokenType.TRUE:        AddToken(type, true); break;
                case TokenType.FALSE:       AddToken(type, false); break;
                case TokenType.NIL:         AddToken(type, null); break;
                default:                    AddToken(type); break;
            }
        } else AddToken(TokenType.IDENTIFIER);

    }

    private void ScanString() {
        // Stop at a newline because strings cannot span multiple lines.
        while (Peek() != '"' && Peek() != '\n' && !IsAtEnd()) { Advance();}
        if (Peek() == '\n' || IsAtEnd()) { FlagError("Unterminated string."); return;}

        Advance(); // consume closing "

        string value = _source[(_start + 1) .. (_current - 1)];
        AddToken(TokenType.STRING, value);
    }

    private void ScanNumber() {
        while (char.IsDigit(Peek())) Advance();

        // check if number is fractional (contains a decimal point)
        if (Peek() == '.' && char.IsDigit(Peek(1))) {
            Advance();      // consume the "."
            while (char.IsDigit(Peek())) Advance();
        }

        double value = double.Parse(_source[_start .. _current], System.Globalization.CultureInfo.InvariantCulture);
        AddToken(TokenType.NUMBER, value);
    }
}
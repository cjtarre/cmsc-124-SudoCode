class Parser {
    // fields and properties
    private readonly IReadOnlyList<Token> _tokens = [];
    private readonly ErrorHandler _error;

    // rework to produce a tree of expressions instead of a list
    private readonly List<Expr> _expr = [];

    // marker references
    private int _current = 0;
    private int? _expressionLine = null;

    // constructor
    public Parser(IReadOnlyList<Token> tokens, ErrorHandler error) {
        _tokens = tokens;
        _error = error;
    }

    // error handling
    private class ParseError : Exception { public ParseError(string message) : base(message) {} }
    private ParseError Error(string message, Token token) {
        _error.Report("Parse", message, token);
        return new ParseError(message);
    }
    
    // helper methods
    private bool Match (params TokenType[] types) { // checks and advances if true
        // do not consume tokens belonging to the next expression
        if (_expressionLine != null && Peek().Line != _expressionLine) {
            return false;
        }

        foreach (TokenType type in types) {
            if (Check(type)) {
                Advance();
                return true;
            }
        }
        return false;
    }
    private bool IsAtEnd() => Peek().Type == TokenType.EOF;
    private bool Check(TokenType type) => Peek().Type == type;
    private Token Peek() => _tokens[_current]; 
    private Token Previous() => _tokens[_current - 1];  // returns the most recently consumed token
    private Token Advance() {
        if (!IsAtEnd()) _current++;
        return Previous();
    }
    private Token Consume(TokenType type, string message) {
        if (_expressionLine != null && Peek().Line != _expressionLine)
            throw Error(message, Peek());
        
        if (Check(type)) return Advance();
        throw Error(message, Peek());
    }

    // core parsing methods
    public IReadOnlyList<Expr> Run() {
        while (!IsAtEnd()) {
            try {
                _expressionLine = Peek().Line;
                _expr.Add(Expression());

                // only one expression is allowed per line
                if (!IsAtEnd() && Peek().Line == _expressionLine) {
                    throw Error("Expected end of expression.", Peek());
                }
            }
            // attempt to recover at the next line
            catch (ParseError) { Synchronize(); }
        }
        return _expr;
    }

    private void Synchronize() {
        Advance();                  // consume the erroneous token
        while (!IsAtEnd()) {        // keep advancing until we find a statement boundary (by line for now)
            if (Previous().Line != Peek().Line) return;
            Advance();
        }
    }

    // grammar rules (operations ordered by precedence from lowest to highest)
    private Expr Expression() => Assignment();  // entry point for parsing expressions
    private Expr Assignment() {
        Expr expr = LogicalOr();                // drop down to the highest precedence level first
        if (Match(TokenType.ASSIGN)) {          // right associative
            Token op = Previous();              // consume the assignment operator
            Expr value = Assignment();          // recursively calls itself to parse the right-hand side of the assignment first
            if (expr is Expr.Variable v) {      // check if the left-hand side is a valid assignment target (a variable)
                return new Expr.Assignment(v.Name, value);
            }
            throw Error("Invalid assignment target.", op);  // if the left-hand side is not a variable, throw an error
        }
        return expr;
    }
    private Expr LogicalOr() {
        Expr expr = LogicalAnd();
        while (Match(TokenType.OR, TokenType.XOR)) {    // left associative
            Token op = Previous();
            Expr right = LogicalAnd();                  // drop down to the next precedence level to parse the right-hand side of the logical operation
            expr = new Expr.Binary(expr, op, right);    // build a new binary expression node with the left and right operands and the operator
        }
        return expr;
    }
    private Expr LogicalAnd() {
        Expr expr = LogicalNot();
        while (Match(TokenType.AND)) {
            Token op = Previous();
            Expr right = LogicalNot();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }
    private Expr LogicalNot() {
        if (Match(TokenType.NOT)) {
            Token op = Previous();
            Expr right = LogicalNot();
            return new Expr.Unary(op, right);
        }
        return Comparison();
    }
    private Expr Comparison() {
        Expr expr = Term();
        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESSER, TokenType.LESSER_EQUAL, TokenType.EQUAL, TokenType.NOT_EQUAL)) {
            Token op = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }
    private Expr Term() {
        Expr expr = Factor();
        while (Match(TokenType.PLUS, TokenType.MINUS)) {
            Token op = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }
    private Expr Factor() {
        Expr expr = UnaryMinus();
        while (Match(TokenType.STAR, TokenType.SLASH, TokenType.MOD)) {
            Token op = Previous();
            Expr right = UnaryMinus();
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }
    private Expr UnaryMinus() {
        if (Match(TokenType.MINUS)) {
            Token op = Previous();
            Expr right = UnaryMinus();
            return new Expr.Unary(op, right);
        }
        return Power();
    }
    private Expr Power() {
        Expr expr = Primary();
        if (Match(TokenType.EXP)) {
            Token op = Previous();
            Expr right = Power(); 
            expr = new Expr.Binary(expr, op, right);
        }
        return expr;
    }

    // evaluates the atomic items in the grammar (literals, identifiers, and parenthesized expressions)
    private Expr Primary() {
        if (Match(TokenType.FALSE))     return new Expr.Literal(false);
        if (Match(TokenType.TRUE))      return new Expr.Literal(true);
        if (Match(TokenType.NIL))       return new Expr.Literal(null);
        if (Match(TokenType.NUMBER, TokenType.STRING)) {
            return new Expr.Literal(Previous().Literal);
        }
        if (Match(TokenType.IDENTIFIER)) {
            return new Expr.Variable(Previous());
        }
        if (Match(TokenType.LEFT_PAREN)) {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
            return new Expr.Grouping(expr);
        }
        throw Error("Expected expression.", Peek());
    }
}
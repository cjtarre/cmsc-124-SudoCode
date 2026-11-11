class Parser {
    // fields and properties
    private readonly IReadOnlyList<Token> _tokens = [];
    private readonly ErrorHandler _error;

    // Track the current token and expression boundaries.
    private int _current = 0;
    private int? _expressionLine = null; // Restrict legacy expressions to one line.

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
    public Expr Run() {
        List<Expr> _expr = [];
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
        return new Expr.Sequence(_expr);
    }

    // Parse a complete Lab 4 program.
    // Allow bare expressions only when running in REPL mode.
    public IReadOnlyList<Statement> RunStatements(bool allowExpressions = false) {
        List<Statement> statements = [];

        // Allow Lab 4 expressions to span multiple lines.
        _expressionLine = null;

        while (!IsAtEnd()) {
            int start = _current;

            try {
                statements.Add(ParseStatement(allowExpressions));
            }
            catch (ParseError) {
                // Ensure recovery always makes progress.
                if (_current == start && !IsAtEnd())
                    Advance();

                // Continue parsing to collect additional syntax errors.
                SynchronizeStatements();
            }
        }

        return statements;
    }

    // Identify the statement from its opening keyword.
    private Statement ParseStatement(bool allowExpressions = false) {
        if (Match(TokenType.INITIALIZE))
            return ParseInitialize();

        if (Match(TokenType.SET))
            return ParseSet();

        if (Match(TokenType.PRINT, TokenType.OUTPUT))
            return new Statement.Print(Expression());

        // BEGIN creates a new nested scope.
        if (Match(TokenType.BEGIN))
            return ParseBlock();

        // Treat bare expressions as statements only in REPL mode.
        if (allowExpressions)
            return new Statement.Expression(Expression());

        throw Error("Expected a statement.", Peek());
    }

    // Parse a variable declaration:
    // INITIALIZE variable <- expression
    private Statement ParseInitialize() {
        // Read the variable name after INITIALIZE.
        Token name = Consume(
            TokenType.IDENTIFIER,
            "Expected variable name after INITIALIZE."
        );

        // Require the assignment operator before the initial value.
        Consume(
            TokenType.ASSIGN,
            "Expected '<-' after variable name."
        );

        // Parse the initial value and create the declaration node.
        return new Statement.Initialize(name, Expression());
    }

    // Parse an assignment to an existing variable:
    // SET variable <- expression
    private Statement ParseSet() {
        // Read the variable name after SET.
        Token name = Consume(
            TokenType.IDENTIFIER,
            "Expected variable name after SET."
        );

        // Require the assignment operator before the new value.
        Consume(
            TokenType.ASSIGN,
            "Expected '<-' after variable name."
        );

        // Parse the new value and create the assignment node.
        return new Statement.Set(name, Expression());
    }

    // Parse statements inside BEGIN ... END.
    // Each nested block handles its own syntax errors.
    private Statement ParseBlock() {
        List<Statement> statements = [];

        while (!Check(TokenType.END) && !IsAtEnd()) {
            int start = _current;

            try {
                statements.Add(ParseStatement());
            }
            catch (ParseError) {
                // Avoid repeatedly parsing the same invalid token.
                if (_current == start && !IsAtEnd())
                    Advance();

                SynchronizeStatements();
            }
        }

        // Every BEGIN must have a matching END.
        Consume(TokenType.END, "Expected END after block.");

        return new Statement.Block(statements);
    }

    private void Synchronize() {
        Advance();                  // consume the erroneous token
        while (!IsAtEnd()) {        // keep advancing until we find a statement boundary (by line for now)
            if (Previous().Line != Peek().Line) return;
            Advance();
        }
    }

    // Identify valid starting tokens for Lab 4 statements.
    private bool IsStatementStart() =>
        Check(TokenType.INITIALIZE) ||
        Check(TokenType.SET) ||
        Check(TokenType.PRINT) ||
        Check(TokenType.OUTPUT) ||
        Check(TokenType.BEGIN);

    // Recover at the next statement or block boundary.
    private void SynchronizeStatements() {
        // Skip invalid tokens until a valid boundary is found.
        while (!IsAtEnd()) {
            if (IsStatementStart() || Check(TokenType.END))
                return;

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
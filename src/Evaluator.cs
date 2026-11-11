using System.Globalization;

class Evaluator {
    // Store either an expression program or a statement program.
    private readonly Expr? _AST;
    private readonly IReadOnlyList<Statement>? _statements;
    private readonly ErrorHandler _error;

    // Track the current scope during execution.
    private SudoEnvironment _environment;

    // Store results for the existing expression evaluator.
    private readonly List<object?> _results = new();

    // Labs 2 and 3: evaluate expressions.
    public Evaluator(Expr ast, ErrorHandler error) {
        _AST = ast;
        _error = error;
        _environment = new SudoEnvironment();
    }

    // Lab 4: execute statements using a shared environment.
    public Evaluator(
        IReadOnlyList<Statement> statements,
        ErrorHandler error,
        SudoEnvironment? environment = null
    ) {
        _statements = statements;
        _error = error;
        _environment = environment ?? new SudoEnvironment();
    }


    // error handling
    private class RuntimeError : Exception { public RuntimeError(string message) : base(message) {} }
    private RuntimeError Error(string message, Token token) {
        _error.Report("Runtime", message, token);
        return new RuntimeError(message);
    }
    private RuntimeError Error(string message) {
        _error.Report("Runtime", message);
        return new RuntimeError(message);
    }

    // Labs 2 and 3: evaluate expressions and collect their results.
    public IReadOnlyList<object?> Run() {
        // Support both a sequence of expressions and a single expression.
        var _expr = _AST is Expr.Sequence s
            ? s.Expressions
            : _AST is not null ? [_AST] : [];

        foreach (Expr expr in _expr) {
            try {
                object? result = Evaluate(expr);
                _results.Add(result);
            }
            catch (RuntimeError) {
                // Stop evaluating after the first runtime error.
                break;
            }
        }

        return _results;
    }

    // Execute a complete Lab 4 statement program.
    public void RunStatements(bool printExpressions = false) {
        if (_statements is null) return;

        try {
            foreach (Statement statement in _statements) {
                // Print bare expression results only in REPL mode.
                if (printExpressions && statement is Statement.Expression expr) {
                    Console.WriteLine(StringMe(Evaluate(expr.Value)));
                }
                else {
                    Execute(statement);
                }
            }
        }
        catch (RuntimeError) {
            // Error already reported. Stop execution.
        }
    }

    // Execute one statement according to its type.
    private void Execute(Statement statement) {
        switch (statement) {
            case Statement.Initialize s:
                // Evaluate the initializer before creating the binding.
                _environment.Define(s.Name.Lexeme, Evaluate(s.Initializer));
                break;

            case Statement.Set s:
                // SET must update an existing variable.
                object? value = Evaluate(s.Value);

                if (!_environment.TryAssign(s.Name.Lexeme, value))
                    throw Error($"Undefined variable '{s.Name.Lexeme}'", s.Name);

                break;

            case Statement.Print s:
                Console.WriteLine(StringMe(Evaluate(s.Value)));
                break;

            case Statement.Expression s:
                // Expression statements do not print in file mode.
                Evaluate(s.Value);
                break;

            case Statement.Block s:
                ExecuteBlock(s.Statements);
                break;
        }
    }

    // Execute a block in a temporary child scope.
    private void ExecuteBlock(IReadOnlyList<Statement> statements) {
        SudoEnvironment previous = _environment;

        try {
            _environment = new SudoEnvironment(previous);

            foreach (Statement statement in statements)
                Execute(statement);
        }
        finally {
            // Restore the enclosing scope even after a runtime error.
            _environment = previous;
        }
    }

    // Evaluate an expression and return its resulting value.
    private object? Evaluate(Expr expr) {
        switch (expr) {
            case Expr.Literal l:    return l.Value;
            case Expr.Grouping g:   return Evaluate(g.Expression);

            case Expr.Variable v:
                if (_environment.TryGet(v.Name.Lexeme, out var envValue))
                    return envValue;

                throw Error($"Undefined variable '{v.Name.Lexeme}'", v.Name);


            case Expr.Assignment a:
            {
                var assignedValue = Evaluate(a.Value);

                // Preserve Lab 3 behavior: assignment can create a binding.
                if (!_environment.TryAssign(a.Name.Lexeme, assignedValue))
                    _environment.Define(a.Name.Lexeme, assignedValue);

                return assignedValue;
            }
            
            case Expr.Unary u:
            {
                var right = Evaluate(u.Right);
                switch (u.Operator.Type)
                {
                    case TokenType.MINUS:   CheckNumberOperand(u.Operator, right);
                                            return -(double)right!;
                    case TokenType.NOT:     return !IsTruthy(right);
                    default:      
                    throw Error($"Unary operator not implemented: {u.Operator.Lexeme}", u.Operator);          
                }
            }


            case Expr.Binary b:
            {
                var left = Evaluate(b.Left);
                var right = Evaluate(b.Right);
                switch (b.Operator.Type)
                {
                    // arithmetic
                    case TokenType.PLUS:
                        if (left is double lNum && right is double rNum)  return lNum + rNum;
                        if (left is string || right is string)  return StringMe(left) + StringMe(right);
                        throw Error($"Operands must be two numbers or two strings for operator '{b.Operator.Lexeme}'", b.Operator);
                    
                    case TokenType.MINUS:       CheckNumberOperands(b.Operator, left, right);
                                                return (double)left! - (double)right!;

                    case TokenType.STAR:        CheckNumberOperands(b.Operator, left, right);
                                                return (double)left! * (double)right!;
                                                
                    case TokenType.SLASH:       CheckNumberOperands(b.Operator, left, right);
                        if ((double)right! == 0) throw Error("Division by zero", b.Operator);
                                                return (double)left! / (double)right!;
                                                
                    case TokenType.MOD:         CheckNumberOperands(b.Operator, left, right);
                        if ((double)right! == 0) throw Error("Division by zero", b.Operator);
                                                return (double)left! % (double)right!;

                    // might return infinity or NaN, should prolly update README 
                    case TokenType.EXP:         CheckNumberOperands(b.Operator, left, right);
                                                return Math.Pow((double)left!, (double)right!);

                    // comparison
                    case TokenType.GREATER:     CheckNumberOperands(b.Operator, left, right);
                                                return (double)left! > (double)right!;

                    case TokenType.GREATER_EQUAL:CheckNumberOperands(b.Operator, left, right);
                                                return (double)left! >= (double)right!;

                    case TokenType.LESSER:      CheckNumberOperands(b.Operator, left, right);
                                                return (double)left! < (double)right!;

                    case TokenType.LESSER_EQUAL:CheckNumberOperands(b.Operator, left, right);
                                                return (double)left! <= (double)right!;

                    // equality
                    case TokenType.EQUAL:       return IsEqual(left, right);
                    case TokenType.NOT_EQUAL:   return !IsEqual(left, right);

                    // logical
                    case TokenType.AND:         return IsTruthy(left) && IsTruthy(right);
                    case TokenType.OR:          return IsTruthy(left) || IsTruthy(right);
                    case TokenType.XOR:         return IsTruthy(left) ^ IsTruthy(right);
                    
                    default: 
                    throw Error($"Binary operator not implemented: {b.Operator.Lexeme}", b.Operator);
                }
            }

            default:
            {   
                // flat out just dont include a line number for now
                throw Error($"Evaluation not implemented for expression type: {expr.GetType().Name}");
            }
        }
    }

    // Convert a runtime value into its printable representation.
    public static string StringMe(object? value) => value switch {
        null => "nil",
        bool b => b.ToString().ToLower(),
        double d => d.ToString("G", CultureInfo.InvariantCulture),
        _ => value.ToString()!
    };

    // Determine whether a value is treated as true in logical operations.
    private bool IsTruthy(object? value) {
        return value switch {
            null => false,
            bool b => b,
            _ => true
        };
    }

    // Compare two values, including nil.
    private bool IsEqual(object? left, object? right) {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;

        return left.Equals(right);
    }
    
    private void CheckNumberOperand(Token op, object? operand) {
        if (operand is double) return;
        throw Error($"Operand must be a number for operator '{op.Lexeme}'", op);
    }

    private void CheckNumberOperands(Token op, object? left, object? right) {
        if (left is double && right is double) return;
        throw Error($"Operands must be numbers for operator '{op.Lexeme}'", op);
    }
}
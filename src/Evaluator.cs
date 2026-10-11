using System.Globalization;
class Evaluator {
    // returns objects 
    private readonly List<object?> _results = new List<object?>();
    private readonly IReadOnlyList<Expr> _expressions = [];
    private List<string> errorMessages = [];
    public bool HasErrors => errorMessages.Count > 0;
    public IReadOnlyList<object?> Results => _results;
    public Evaluator(IReadOnlyList<Expr> expressions) => _expressions = expressions;

    private class EvaluationErrorException : Exception {
        public EvaluationErrorException(string message) : base(message) {}
    }

    private EvaluationErrorException Error(string message, Token? t = null) {
        string Message() => "Runtime Error: " + message;
        if (t != null)  errorMessages.Add($"[Line {t.Line}] {Message()}");
        else            errorMessages.Add($"{Message()}");
        return new EvaluationErrorException(message);
    }

    public void Run()
    {
        foreach (Expr expr in _expressions)
        {
            try {
                object? result = Evaluate(expr);
                _results.Add(result);
            }
            catch (EvaluationErrorException) {
                ;
            }
        }   
    }

    public void PrintResults(int mode = 0) 
    {
        if (HasErrors) {
            foreach (string error in errorMessages) Console.Error.WriteLine(error);     
            if (mode == 0) Environment.Exit(70);
            return;
        }
        foreach (object? result in _results) Console.WriteLine(StringMe(result));
    }
    private object? Evaluate(Expr expr)
    {
        switch (expr)
        {
            case Expr.Literal l:    return l.Value;
            case Expr.Grouping g:   return Evaluate(g.Expression);

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

                    default: 
                    throw Error($"Binary operator not implemented: {b.Operator.Lexeme}", b.Operator);
                }
            }

            default:
            throw Error($"Evaluation not implemented for expression type: {expr.GetType().Name}");
        }
    }

    private string StringMe(object? value) => value switch
    {
        null => "nil",
        bool b => b.ToString().ToLower(),
        double d => d.ToString("G", CultureInfo.InvariantCulture),
        _ => value.ToString()!
    };
    
    private bool IsTruthy(object? value) {
        return value switch {
            null => false,
            bool b => b,
            _ => true
        };
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
using System.Globalization;

class Evaluator {
    // returns objects 
    private readonly List<object?> _results = new List<object?>();
    private readonly Expr _AST;
    private readonly ErrorHandler _error;

    // constructor
    public Evaluator(Expr ast, ErrorHandler error) {
        _AST = ast;
        _error = error;
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

    public IReadOnlyList<object?> Run()
    {
        var _expr = _AST is Expr.Sequence s ? s.Expressions : [_AST];
        foreach (Expr expr in _expr)
        {
            try {
                object? result = Evaluate(expr);
                _results.Add(result);
            }
            catch (RuntimeError) {
                ;
            }
        }
        return _results;
    }

    private object? Evaluate(Expr expr)
    {
        switch (expr) {
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
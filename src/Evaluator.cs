class Evaluator {
    // returns objects 
    private readonly List<object?> _results = new List<object?>();
    private readonly IReadOnlyList<Expr> _expressions = [];
    public IReadOnlyList<object?> Results => _results;
    public Evaluator(IReadOnlyList<Expr> expressions) => _expressions = expressions;
    public void Run()
    {
        foreach (Expr expr in _expressions)
        {
                object? result = Evaluate(expr);
                _results.Add(result);
        }   
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
                    throw new NotImplementedException($"Unary operator not implemented: {u.Operator.Lexeme}");
                }
            }


            case Expr.Binary b:
                var left = Evaluate(b.Left);
                var rightBinary = Evaluate(b.Right);
                switch (b.Operator.Type)
                {
                    case TokenType.PLUS:
                        if (left is double lNum && rightBinary is double rNum)  return lNum + rNum;
                        if (left is string lStr && rightBinary is string rStr)  return lStr + rStr;
                        throw new InvalidOperationException($"Operands must be two numbers or two strings for operator '{b.Operator.Lexeme}'");
                    
                    case TokenType.MINUS:       CheckNumberOperands(b.Operator, left, rightBinary);
                                                return (double)left! - (double)rightBinary!;

                    case TokenType.STAR:        CheckNumberOperands(b.Operator, left, rightBinary);
                                                return (double)left! * (double)rightBinary!;
                                                
                    case TokenType.SLASH:       CheckNumberOperands(b.Operator, left, rightBinary);
                        if ((double)rightBinary! == 0) throw new InvalidOperationException("Division by zero");
                                                return (double)left! / (double)rightBinary!;
                                                
                    case TokenType.MOD:         CheckNumberOperands(b.Operator, left, rightBinary);
                        if ((double)rightBinary! == 0) throw new InvalidOperationException("Division by zero");
                                                return (double)left! % (double)rightBinary!;

                    case TokenType.EXP:         CheckNumberOperands(b.Operator, left, rightBinary);
                                                return Math.Pow((double)left!, (double)rightBinary!);

                    default:
                        throw new NotImplementedException($"Binary operator not implemented: {b.Operator.Lexeme}");
                }

            default:
                throw new NotImplementedException($"Evaluation not implemented for expression type: {expr.GetType().Name}");
        }
    }
    
    private bool IsTruthy(object? value) {
        return value switch {
            null => false,
            bool b => b,
            _ => true
        };
    }

    private void CheckNumberOperand(Token op, object? operand) {
        if (operand is double) return;
        throw new InvalidOperationException($"Operand must be a number for operator '{op.Lexeme}'");
    }

    private void CheckNumberOperands(Token op, object? left, object? right) {
        if (left is double && right is double) return;
        throw new InvalidOperationException($"Operands must be numbers for operator '{op.Lexeme}'");
    }
}
class Evaluator {
    // returns objects 
    public object? Evaluate(Expr expr)
    {
        switch (expr)
        {
            case Expr.Literal l:    return l.Value;
            case Expr.Grouping g:   return Evaluate(g.Expression);

            case Expr.Unary u:
                var right = Evaluate(u.Right);
                switch (u.Operator.Type)
                {
                    case TokenType.MINUS:   CheckNumberOperand(u.Operator, right);
                                            return -(double)right!;
                    case TokenType.NOT:     return !IsTruthy(right);
                    default:                
                    throw new NotImplementedException($"Unary operator not implemented: {u.Operator.Lexeme}");
                };

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
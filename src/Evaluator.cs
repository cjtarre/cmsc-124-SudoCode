class Evaluator {
    // returns objects 
    public object? Evaluate(Expr expr)
    {
        switch (expr)
        {
            case Expr.Literal l:    return l.Value;
            case Expr.Grouping g:   return Evaluate(g.Expression);
            default:
                throw new NotImplementedException($"Evaluation not implemented for expression type: {expr.GetType().Name}");
        }
    }
}
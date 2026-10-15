using System.Globalization;
// basically the CFG table
// also contains the AST node definitions
public abstract record Expr {
    // constructors for different expression types (ref: Module)
    public record Sequence(IReadOnlyList<Expr> Expressions) : Expr {}
    public record Literal(object? Value) : Expr;
    public record Variable(Token Name) : Expr;
    public record Unary(Token Operator, Expr Right) : Expr;
    public record Binary(Expr Left, Token Operator, Expr Right) : Expr;
    public record Grouping(Expr Expression) : Expr;
    public record Assignment(Token Name, Expr Value) : Expr;
    public static string Print(Expr expr) => expr switch
    {
        Literal l       => l.Value switch {
            null => "nil",
            bool b => b.ToString().ToLower(),
            double d => (d % 1 == 0) 
                ? d.ToString("F1", CultureInfo.InvariantCulture)
                : d.ToString("G", CultureInfo.InvariantCulture),
            _ => l.Value.ToString()!
        },
        Variable v      => v.Name.Lexeme,
        Grouping g      => $"(group {Print(g.Expression)})",
        Unary u         => $"({u.Operator.Lexeme} {Print(u.Right)})",
        Binary b        => $"({b.Operator.Lexeme} {Print(b.Left)} {Print(b.Right)})",
        Assignment a    => $"(assign {a.Name.Lexeme} {Print(a.Value)})",
        Sequence s      => string.Join("\n", s.Expressions.Select(Print)),
        _ => throw new NotImplementedException($"Print not implemented for {expr.GetType().Name}")
    };
}

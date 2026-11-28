
public abstract record Statement
{
    // Evaluate an expression as a statement.
    public record Expression(Expr Value) : Statement;

    // Declare a variable with an initial value.
    public record Initialize(Token Name, Expr Initializer) : Statement;

    // Update an existing variable.
    public record Set(Token Name, Expr Value) : Statement;

    // Display the result of an expression.
    public record Print(Expr Value) : Statement;

    // Group statements within a nested scope.
    public record Block(IReadOnlyList<Statement> Statements) : Statement;
}

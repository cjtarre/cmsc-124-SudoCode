enum ExitCode{
    Success = 0,
    StaticError = 65,
    RuntimeError = 70
}

// error formatting as immutable record
public record Error(string k, string m, Token? t = null) {
    public override string ToString() =>
        t != null
            ? $"[Line {t.Line}] {k} Error: {m}"
            : $"{k} Error: {m}";
}

// error handler class
class ErrorHandler {
    private readonly List<Error> _items = [];
    public IReadOnlyList<Error> Items => _items;
    public bool HasErrors => _items.Count > 0;
    public void Report(string kind, string message, Token? token = null) =>
        _items.Add(new Error(kind, message, token));
}
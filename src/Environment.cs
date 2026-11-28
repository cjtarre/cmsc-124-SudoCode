public class SudoEnvironment {
	// Store current variables 
    private readonly Dictionary<string, object?> _values = new();

    public SudoEnvironment? Enclosing { get; }
    public SudoEnvironment(SudoEnvironment? enclosing = null) => Enclosing = enclosing;

    // Create a binding in the current scope if it does not already exist.
    public bool TryDefine(string name, object? value) => _values.TryAdd(name, value);

    // Search the current scope, then enclosing scopes.
    public bool TryGet(string name, out object? value) {
        if (_values.TryGetValue(name, out value))
            return true;

        value = null;
        return Enclosing?.TryGet(name, out value) ?? false;
    }

    // Update the nearest existing binding.
    public bool TryAssign(string name, object? value) {
        if (_values.ContainsKey(name)) {
            _values[name] = value;
            return true;
        }
        return Enclosing?.TryAssign(name, value) ?? false;
    }
}

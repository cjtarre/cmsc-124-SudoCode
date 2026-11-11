
using System.Collections.Generic;

public class SudoEnvironment
{
    private readonly Dictionary<string, object?> _values = new();

    public SudoEnvironment? Enclosing { get; }

    public SudoEnvironment(SudoEnvironment? enclosing = null)
    {
        Enclosing = enclosing;
    }

    // Create or replace a binding in the current scope.
    public void Define(string name, object? value)
    {
        _values[name] = value;
    }

    // Search the current scope, then enclosing scopes.
    public bool TryGet(string name, out object? value)
    {
        if (_values.TryGetValue(name, out value))
            return true;

        if (Enclosing != null)
            return Enclosing.TryGet(name, out value);

        value = null;
        return false;
    }

    // Update the nearest existing binding.
    public bool TryAssign(string name, object? value)
    {
        if (_values.ContainsKey(name))
        {
            _values[name] = value;
            return true;
        }

        if (Enclosing != null)
            return Enclosing.TryAssign(name, value);

        return false;
    }
}

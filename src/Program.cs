using System;
using System.IO;
using System.Text;

static void Fail(string message){
    Console.Error.WriteLine($"SudoCode: {message}");
    Environment.Exit(65);
}

static string? LoadSource(string path) {
    try {
        return File.ReadAllText(path, Encoding.UTF8);
    } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
        Fail($"cannot read '{path}': {error.Message}");
    } catch (Exception error) {
        Fail($"error while reading '{path}': {error.Message}");
    }
    return null;
}

static Scanner? ScanSource(string source) {
    try {
        Scanner scanner = new Scanner(source);
        scanner.ScanTokens();
        return scanner;
    } catch (Exception error) {
        Fail($"error while scanning: {error.Message}");
        return null;
    }
}
static Scanner? Scan(string path) {
    string? source = LoadSource(path);
    if (source == null) return null;
    return ScanSource(source);
}
static Parser? ParseSource(string source) {
    Scanner? scanner = ScanSource(source);
    if (scanner == null) return null;
    if (scanner.HasErrors) {
        foreach (string error in scanner.ErrorMessages) Console.Error.WriteLine(error);
        return null;
    }
    try {
        Parser parser = new Parser(scanner.Tokens);
        parser.Parse();
        return parser;
    } catch (Exception error) {
        Fail($"error while parsing: {error.Message}");
        return null;
    }
}
static Parser? Parse(string path) {
    string? source = LoadSource(path);
    if (source == null) return null;
    return ParseSource(source);
}

static Evaluator? EvaluateSource(string source) {
    Parser? parser = ParseSource(source);
    if (parser == null) return null;
    if (parser.HasErrors) {
        foreach (string error in parser.ErrorMessages) Console.Error.WriteLine(error);
        return null;
    }
    try {
        Evaluator evaluator = new Evaluator(parser.Expressions);
        evaluator.Run();
        return evaluator;

    } catch (Exception error) {
        Fail($"error while evaluating: {error.Message}");
        return null;
    }
}

static Evaluator? Evaluate(string path) {
    string? source = LoadSource(path);
    if (source == null) return null;
    return EvaluateSource(source);
}


static void REPL() {
    Console.WriteLine("SudoCode REPL 0.1.0 (Press Ctrl+C to exit)");
    while (true) {
        Console.Write("> ");
        string? line = Console.ReadLine();
        if (line == null) break;
        Evaluator? evaluator = EvaluateSource(line);
        evaluator?.PrintResults(1);
    }
}

// Main program entry point
if (args.Length == 0) {
    REPL();
    return 0;
}
string path;
switch (args[0]) {
    case "--tokenize":
        if (args.Length != 2) Fail("expected --tokenize <source-file>");
        path = args[1];
        Scanner? scanner = Scan(path);
        scanner?.PrintTokens();
        break;
    case "--parse":
        if (args.Length != 2) Fail("expected --parse <source-file>");
        path = args[1];
        Parser? parser = Parse(path);
        if (parser == null) Environment.Exit(65);
        parser?.PrintAST();
        break;
    case "--eval":
        if (args.Length != 2) Fail("expected --eval <source-file>");
        path = args[1];
        Evaluator? evaluator = Evaluate(path);
        if (evaluator == null) Environment.Exit(65);
        evaluator?.PrintResults();
        break;
    case "--version":
        Console.WriteLine("SudoCode 0.1.0");
        break;
    default:
        if (args.Length != 1) {
            Fail("expected a source-file path");
        }
        path = args[0];
        try {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Write(File.ReadAllText(path, Encoding.UTF8));
            Environment.Exit(0);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            Fail($"cannot read '{path}': {error.Message}");
        } 
        break;
}
return 0;
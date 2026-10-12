using System;
using System.IO;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

ExitCode code = args switch {
    []                      => REPL(),
    ["--tokenize",  var p]  => LoadSource(p, Stage.Tokenize),
    ["--parse",     var p]  => LoadSource(p, Stage.Parse),
    ["--eval",      var p]  => LoadSource(p, Stage.Eval),
    [var p]                 => Echo(p),   
    _                       => Usage(args)
};
return (int)code;

static ExitCode Execute(string source, Stage stage) {
    try { return Pipeline.Run(source, stage); }
    catch (Exception e) {
        Console.Error.WriteLine($"SudoCode: {e.Message}");
        return ExitCode.RuntimeError;
    }
}

static ExitCode LoadSource(string path, Stage stage) {
    try { return Execute(File.ReadAllText(path, Encoding.UTF8), stage); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
        Console.Error.WriteLine($"SudoCode: cannot read '{path}': {error.Message}");
        return ExitCode.StaticError;
    } 
}

static ExitCode REPL() {
    string? Prompt() {
        Console.Write("> ");
        return Console.ReadLine();
    }

    Console.WriteLine("SudoCode REPL 0.1.0 (Press Ctrl+C to exit)");
    while (Prompt() is {} line ) Execute(line, Stage.Eval);
                                                // change Stage to any desired lab 
    return ExitCode.Success;
}

static ExitCode Usage(string[] args) {
    switch (args.Length)
    {   
        case 0:
            Console.WriteLine("SudoCode: no arguments provided");
            break;
        case 2 when args[0] is not ("--tokenize" or "--parse" or "--eval"):
            Console.WriteLine($"SudoCode: unknown option '{args[0]}'");
            break;
        case 2 when !File.Exists(args[1]):
            Console.WriteLine($"SudoCode: file '{args[1]}' does not exist");
            break;
    }
    Console.WriteLine("Usage: sudo [ --tokenize | --parse | --eval ] <source-file>");
    return ExitCode.StaticError;
}

static ExitCode Echo(string source) {
    try {
        Console.Write(File.ReadAllText(source, Encoding.UTF8));
        return ExitCode.Success;
    } catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"SudoCode: cannot read '{source}': {error.Message}");
        return ExitCode.StaticError;
    }
}
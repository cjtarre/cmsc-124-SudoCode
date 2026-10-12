using System;
using System.IO;
using System.Text;


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

    }
}

}

}


static void REPL() {
    Console.WriteLine("SudoCode REPL 0.1.0 (Press Ctrl+C to exit)");
    while (true) {
        Console.Write("> ");
        string? line = Console.ReadLine();
        if (line == null) break;
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
    case "--parse":
    case "--eval":
        if (args.Length != 2) Fail("expected --eval <source-file>");
        path = args[1];
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
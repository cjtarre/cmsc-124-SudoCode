
using System;
using System.IO;
using System.Text;

// Use UTF-8 to support Unicode characters in program output.
Console.OutputEncoding = Encoding.UTF8;

// Select the execution mode based on the command-line arguments.
ExitCode code = args switch {
    []                      => REPL(),
    ["--tokenize",  var p]  => LoadSource(p, Stage.Tokenize),
    ["--parse",     var p]  => LoadSource(p, Stage.Parse),
    ["--eval",      var p]  => LoadSource(p, Stage.Eval),
    [var p]                 => LoadSource(p, Stage.Program),
    _                       => Usage(args)
};

// Return the exit code to the operating system.
return (int)code;

// Execute source code through the selected pipeline stage.
static ExitCode Execute(
    string source,
    Stage stage,
    SudoEnvironment? environment = null
) {
    try {
        return Pipeline.Run(source, stage, environment);
    }
    catch (Exception e) {
        // Report unexpected execution errors without crashing the program.
        Console.Error.WriteLine($"SudoCode: {e.Message}");
        return ExitCode.RuntimeError;
    }
}

// Read a source file and pass its contents to the execution pipeline.
static ExitCode LoadSource(string path, Stage stage) {
    try { return Execute(File.ReadAllText(path, Encoding.UTF8), stage); }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
        // Report file access errors using the static error exit code.
        Console.Error.WriteLine($"SudoCode: cannot read '{path}': {error.Message}");
        return ExitCode.StaticError;
    }
}

// Run an interactive session that accepts statements and expressions.
static ExitCode REPL() {
    // Create a shared environment to preserve variables between inputs.
    var environment = new SudoEnvironment();

    // Store multiple lines until a complete block is entered.
    var buffer = new StringBuilder();

    // Track the number of BEGIN blocks that have not been closed.
    int blockDepth = 0;

    Console.WriteLine("SudoCode REPL 0.1.0 (Type 'exit' to quit)");

    while (true) {
        // Show a continuation prompt when inside an unfinished block.
        Console.Write(blockDepth > 0 ? "... " : "> ");
        string? line = Console.ReadLine();

        // Exit when the input stream reaches its end.
        if (line is null)
            break;

        // Allow the user to exit when no block is open.
        if (blockDepth == 0 &&
            line.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            break;

        // Ignore empty or whitespace-only lines.
        if (string.IsNullOrWhiteSpace(line))
            continue;

        // Add the current line to the pending source code.
        buffer.AppendLine(line);

        // Split the line into words to detect block delimiters.
        foreach (string word in line.Split(
            new[] { ' ', '\t', '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries
        )) {
            // A new BEGIN opens another block.
            if (word.Equals("BEGIN", StringComparison.OrdinalIgnoreCase))
                blockDepth++;

            // END closes the most recent open block.
            else if (word.Equals("END", StringComparison.OrdinalIgnoreCase))
                blockDepth--;
        }

        // Wait for more input while a block remains open.
        if (blockDepth > 0)
            continue;

        // Parse and execute the complete input using the shared environment.
        Execute(buffer.ToString(), Stage.Repl, environment);

        // Clear the buffer and reset the depth for the next input.
        buffer.Clear();
        blockDepth = 0;
    }

    // End the interactive session successfully.
    return ExitCode.Success;
}

// Display usage information when the command-line arguments are invalid.
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

    // Show the supported command format.
    Console.WriteLine("Usage: sudo [ --tokenize | --parse | --eval ] <source-file>");
    return ExitCode.StaticError;
}

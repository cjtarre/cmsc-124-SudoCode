// Define the supported execution modes.
enum Stage { Tokenize, Parse, Eval, Program, Repl }

static class Pipeline {
    // Process source code according to the selected execution stage.
    public static ExitCode Run(
        string source,
        Stage stage,
        SudoEnvironment? environment = null
    ) {
        // Collect errors encountered during scanning, parsing, or evaluation.
        var e = new ErrorHandler();

        // Report errors before moving to the next stage.
        ExitCode? Gate(ExitCode failcon) =>
            e.HasErrors ? Finish(e, failcon) : null;

        // Scan the source.
        var tokens = new Scanner(source, e).Run();

        // Stop if the scanner reports a lexical error.
        if (Gate(ExitCode.StaticError) is {} scanFailed)
            return scanFailed;

        // Return tokens directly when running the tokenizer.
        if (stage == Stage.Tokenize)
            return Finish(tokens);

        // Lab 4: Parse and execute statements.
        if (stage is Stage.Program or Stage.Repl) {
            // REPL mode allows bare expressions and prints their results.
            bool isRepl = stage == Stage.Repl;

            // Parse the complete input into statements.
            var statements = new Parser(tokens, e).RunStatements(isRepl);

            // Do not execute any statements if parsing fails.
            if (Gate(ExitCode.StaticError) is {} parseFailed)
                return parseFailed;

            // Execute statements using the supplied or a new environment.
            var evaluator = new Evaluator(statements, e, environment);
            evaluator.RunStatements(printExpressions: isRepl);

            // Stop and report any runtime errors.
            if (Gate(ExitCode.RuntimeError) is {} evalFailed)
                return evalFailed;

            return ExitCode.Success;
        }

        // Labs 2–3: Preserve the existing expression pipeline.
        var ast = new Parser(tokens, e).Run();

        // Report syntax errors before evaluating expressions.
        if (Gate(ExitCode.StaticError) is {} legacyParseFailed)
            return legacyParseFailed;

        // Display the expression AST in parse mode.
        if (stage == Stage.Parse)
            return Finish(Expr.Print(ast));

        // Evaluate expressions and print their results.
        var results = new Evaluator(ast, e).Run();
        Finish(results.Select(Evaluator.StringMe).ToList());

        // Report runtime errors after evaluation.
        if (Gate(ExitCode.RuntimeError) is {} legacyEvalFailed)
            return legacyEvalFailed;

        return ExitCode.Success;
    }

    // error printer finish
    private static ExitCode Finish(ErrorHandler e, ExitCode failcon) {
        // Print diagnostics to stderr instead of normal program output.
        foreach (var error in e.Items) Console.Error.WriteLine(error);
        return e.HasErrors ? failcon : ExitCode.Success;
    }

    // string printer finish
    private static ExitCode Finish(string output) {
        Console.WriteLine(output);
        return ExitCode.Success;
    }

    // generic printer finish
    private static ExitCode Finish<T>(IReadOnlyList<T> items) {
        // Print each token, expression result, or other output item.
        foreach (var item in items) Console.WriteLine(item);
        return ExitCode.Success;
    }
}

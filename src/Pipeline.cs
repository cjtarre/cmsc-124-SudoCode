enum Stage { Tokenize, Parse, Eval }

static class Pipeline {
    public static ExitCode Run(string source, Stage stage) {
        var e = new ErrorHandler();

        // error gate
        ExitCode? Gate(ExitCode failcon) => 
            e.HasErrors ? Finish(e, failcon) : null;

        // tokenize
        var tokens = new Scanner(source, e).Run();
        if (Gate(ExitCode.StaticError) is {} scanFailed) return scanFailed;
        if (stage == Stage.Tokenize)    return Finish(tokens);
        
        // parse
        var ast = new Parser(tokens, e).Run();
        if (Gate(ExitCode.StaticError) is {} parseFailed) return parseFailed;
        if (stage == Stage.Parse)       return Finish(Expr.Print(ast));

        // eval
        var res = new Evaluator(ast, e).Run();
        Finish(res.Select(Evaluator.StringMe).ToList());
        if (Gate(ExitCode.RuntimeError) is {} evalFailed) return evalFailed;
        return ExitCode.Success;
    }

    // error printer finish
    private static ExitCode Finish(ErrorHandler e, ExitCode failcon) {
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
        foreach (var item in items) Console.WriteLine(item);
        return ExitCode.Success;
    }
}
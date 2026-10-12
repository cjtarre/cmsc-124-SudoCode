enum Stage { Tokenize, Parse, Eval }

static class Pipeline {
    public static ExitCode Run(string source, Stage stage) {
        var e = new ErrorHandler();

        // error gate
        ExitCode? Gate(ExitCode failcon) => 
            e.HasErrors ? Finish(e, failcon) : null;

        return ExitCode.Success;
    }
}
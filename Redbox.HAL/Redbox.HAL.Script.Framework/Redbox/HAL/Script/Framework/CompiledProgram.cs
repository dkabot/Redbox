namespace Redbox.HAL.Script.Framework
{
    internal sealed class CompiledProgram
    {
        internal readonly ExecutionResult CompilerResult;
        internal readonly ProgramInstruction ParseTree;
        internal bool VisibleToUser;

        internal CompiledProgram(ExecutionResult compileResult, ProgramInstruction tree)
        {
            ParseTree = tree;
            CompilerResult = compileResult;
            VisibleToUser = true;
        }

        public override string ToString()
        {
            return CompilerResult.Errors.ErrorCount == 0 && CompilerResult.Errors.WarningCount == 0
                ? "Cached"
                : string.Format("Errors: {0}, Warnings: {1}", CompilerResult.Errors.ErrorCount,
                    CompilerResult.Errors.WarningCount);
        }
    }
}
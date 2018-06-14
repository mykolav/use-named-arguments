namespace UseNamedArguments.Tests.Support.CodeFix
{
    internal class UseNamedArgsCSharpCodeFixRunner
    {
        /// <summary>
        /// Called to test a C# codefix when applied on the inputted string as a source
        /// </summary>
        /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        public static void InvokeAndVerifyResult(
            string oldSource, 
            string newSource, 
            int? codeFixIndex = null, 
            bool allowNewCompilerDiagnostics = false)
        {
            CodeFixRunnerAndVerifier.InvokeAndVerifyCSharpFix(
                new UseNamedArgsForParamsOfSameTypeCodeFixProvider(),
                new UseNamedArgsForParamsOfSameTypeAnalyzer(),
                oldSource,
                newSource,
                codeFixIndex,
                allowNewCompilerDiagnostics
            );
        }
    }
}
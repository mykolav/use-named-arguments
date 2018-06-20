namespace UseNamedArguments.Tests.Support.CodeFix
{
    internal class UseNamedArgsCSharpCodeFixRunner
    {
        private const string Placeholder = "<PLACEHOLDER>";
        private static readonly string SourceTemplate = $@"
            namespace Frobnitz
            {{
                class Wombat
                {{
                    {Placeholder}
                }}
            }}
        ";

        /// <summary>
        /// Called to test a C# codefix when applied on the inputted string as a source
        /// </summary>
        /// <param name="originalSourceSnippet">A class in the form of a string before the CodeFix was applied to it</param>
        /// <param name="expectedSourceSnippet">A class in the form of a string after the CodeFix was applied to it</param>
        /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        public static void InvokeAndVerifyResult(
            string originalSourceSnippet, 
            string expectedSourceSnippet, 
            int? codeFixIndex = null, 
            bool allowNewCompilerDiagnostics = false)
        {
            var originalSource = FormatSource(originalSourceSnippet);
            var expectedSource = FormatSource(expectedSourceSnippet);

            CodeFixRunnerAndVerifier.InvokeAndVerifyCSharpFix(
                new UseNamedArgsForParamsOfSameTypeCodeFixProvider(),
                new UseNamedArgsForParamsOfSameTypeAnalyzer(),
                originalSource,
                expectedSource,
                codeFixIndex,
                allowNewCompilerDiagnostics
            );
        }

        private static string FormatSource(string snippet)
        { 
            return SourceTemplate.Replace(Placeholder, snippet);
        }
    }
}
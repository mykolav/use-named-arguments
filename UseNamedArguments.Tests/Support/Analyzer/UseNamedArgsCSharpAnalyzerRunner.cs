using UseNamedArguments.Tests.Support.Analyzer;
using UseNamedArguments.Tests.Support.Analyzer.Diagnostics;

namespace UseNamedArguments.Tests.Support
{
    internal static class UseNamedArgsCSharpAnalyzerRunner
    {
        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on</param>
        /// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source</param>
        public static void InvokeAndVerifyDiagnostic(string source, params DiagnosticResult[] expected)
        {
            DiagnosticAnalyzerRunnerAndVerifier.InvokeCSharpAnalyzerAndVerifyDiagnostic(
                new UseNamedArgsForParamsOfSameTypeAnalyzer(),
                source,
                expected
            );
        }
    }
}

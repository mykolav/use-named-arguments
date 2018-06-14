using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UseNamedArguments.Tests.Support.Analyzer.Diagnostics;

namespace UseNamedArguments.Tests.Support.Analyzer
{
    /// <summary>
    /// Superclass of all Unit Tests for DiagnosticAnalyzers
    /// </summary>
    public static class DiagnosticAnalyzerRunnerAndVerifier
    {
        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on</param>
        /// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source</param>
        public static void InvokeCSharpAnalyzerAndVerifyDiagnostic(
            DiagnosticAnalyzer cSharpdiAnalyzer,
            string source, 
            params DiagnosticResult[] expected)
        {
            InvokeAnalyzerAndVerifyDiagnostics(new[] { source }, LanguageNames.CSharp, cSharpdiAnalyzer, expected);
        }

        /// <summary>
        /// Called to test a VB DiagnosticAnalyzer when applied on the single inputted string as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the source</param>
        public static void InvokeBasicAnalyzerAndVerifyDiagnostic(
            DiagnosticAnalyzer visualBasicAnalyzer,
            string source, 
            params DiagnosticResult[] expected)
        {
            InvokeAnalyzerAndVerifyDiagnostics(new[] { source }, LanguageNames.VisualBasic, visualBasicAnalyzer, expected);
        }

        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        public static void InvokeCSharpAnalyzerAndVerifyDiagnostic(
            DiagnosticAnalyzer cSharpAnalyzer,
            string[] sources, 
            params DiagnosticResult[] expected)
        {
            InvokeAnalyzerAndVerifyDiagnostics(sources, LanguageNames.CSharp, cSharpAnalyzer, expected);
        }

        /// <summary>
        /// Called to test a VB DiagnosticAnalyzer when applied on the inputted strings as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        public static void InvokeBasicAnalyzerAndVerifyDiagnostic(
            DiagnosticAnalyzer visualBasicAnalyzer,
            string[] sources, 
            params DiagnosticResult[] expected)
        {
            InvokeAnalyzerAndVerifyDiagnostics(sources, LanguageNames.VisualBasic, visualBasicAnalyzer, expected);
        }

        /// <summary>
        /// General method that gets a collection of actual diagnostics found in the source after the analyzer is run, 
        /// then verifies each of them.
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="language">The language of the classes represented by the source strings</param>
        /// <param name="analyzer">The analyzer to be run on the source code</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        private static void InvokeAnalyzerAndVerifyDiagnostics(
            string[] sources, 
            string language, 
            DiagnosticAnalyzer analyzer, 
            params DiagnosticResult[] expected)
        {
            var diagnostics = analyzer.GetSortedDiagnostics(language, sources);
            DiagnosticsVerifier.VerifyResults(diagnostics, analyzer, expected);
        }
    }
}

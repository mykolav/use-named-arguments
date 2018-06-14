using UseNamedArguments.Tests.Support;
using UseNamedArguments.Tests.Support.Analyzer.Diagnostics;
using Xunit;

namespace UseNamedArguments.Tests
{
    public class UseNamedArgumentsAnalyzerTests
    {
        [Fact]
        public void Empty_code_does_not_trigger_diagnostic()
        {
            const string testCodeSnippet = @"";

            //No diagnostics expected to show up
            var emptyExpectedDiagnostics = new DiagnosticResult[] { };
            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, emptyExpectedDiagnostics);
        }

        [Fact]
        public void Method_with_zero_args_does_not_trigger_diagnostic()
        {
            var testCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork() {}
                        void Bork()
                        {
                            Gork();
                        }
                    }
                }
            ";

            //No diagnostics expected to show up
            var emptyExpectedDiagnostics = UseNamedArgumentsDiagnosticResult.EmptyExpectedDiagnostics;
            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, emptyExpectedDiagnostics);
        }

        [Fact]
        public void Method_with_one_param_does_not_trigger_diagnostic()
        {
            var testCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(int powerLevel) {}
                        void Bork()
                        {
                            Gork(9001);
                        }
                    }
                }
            ";

            //No diagnostics expected to show up
            var emptyExpectedDiagnostics = UseNamedArgumentsDiagnosticResult.EmptyExpectedDiagnostics;
            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, emptyExpectedDiagnostics);
        }

        [Fact]
        public void Method_with_variable_number_of_params_does_not_trigger_diagnostic()
        {
            var testCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(int line, int column, params string[] diagnosticMessages) {}
                        void Bork()
                        {
                            Gork(9000, 1, ""Goku"");
                        }
                    }
                }
            ";

            //No diagnostics expected to show up
            var emptyExpectedDiagnostics = UseNamedArgumentsDiagnosticResult.EmptyExpectedDiagnostics;
            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, emptyExpectedDiagnostics);
        }

        [Fact]
        public void Method_with_different_type_params_invoked_with_positional_args_does_not_trigger_diagnostic()
        {
            const string testCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(string name, int powerLevel) {}
                        void Bork()
                        {
                            Gork(""Goku"", 9001);
                        }
                    }
                }
            ";

            var emptyExpectedDiagnostics = UseNamedArgumentsDiagnosticResult.EmptyExpectedDiagnostics;
            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, emptyExpectedDiagnostics);
        }

        [Fact]
        public void Method_with_same_type_params_invoked_with_vars_named_same_as_args_does_not_trigger_diagnostic()
        {
            const string testCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(string fileName, int line, int column) {}
                        void Bork()
                        {
                            var line = 9000;
                            var column = 1;
                            Gork(fileName: ""Gizmo.cs"", line, column);
                        }
                    }
                }
            ";

            var emptyExpectedDiagnostics = UseNamedArgumentsDiagnosticResult.EmptyExpectedDiagnostics;
            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, emptyExpectedDiagnostics);
        }

        [Fact]
        public void Method_with_same_type_params_invoked_with_named_args_does_not_trigger_diagnostic()
        {
            const string testCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(string fileName, int line, int column) {}
                        void Bork()
                        {
                            Gork(fileName: ""Gizmo.cs"", line: 9000, column: 1);
                        }
                    }
                }
            ";

            var emptyExpectedDiagnostics = UseNamedArgumentsDiagnosticResult.EmptyExpectedDiagnostics;
            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, emptyExpectedDiagnostics);
        }

        [Fact]
        public void Method_with_same_type_params_invoked_with_named_args_starting_from_same_type_params_does_not_trigger_diagnostic()
        {
            const string testCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(string fileName, int line, int column) {}
                        void Bork()
                        {
                            Gork(""Gizmo.cs"", line: 9000, column: 1);
                        }
                    }
                }
            ";

            var emptyExpectedDiagnostics = UseNamedArgumentsDiagnosticResult.EmptyExpectedDiagnostics;
            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, emptyExpectedDiagnostics);
        }

        [Fact]
        public void Method_with_same_type_params_invoked_with_positional_args_triggers_diagnostic()
        {
            const string testCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(string fileName, int line, int column) {}
                        void Bork()
                        {
                            Gork(""Gizmo.cs"", 9000, 1);
                        }
                    }
                }
            ";

            var expectedDiagnostic = UseNamedArgumentsDiagnosticResult.Create(
                "Gork",
                new [] { new [] { "line", "column" } },
                new DiagnosticResultLocation("Test0.cs", line: 9, column: 29));

            UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(testCodeSnippet, expectedDiagnostic);
        }
    }
}
using UseNamedArguments.Tests.Support;
using UseNamedArguments.Tests.Support.Analyzer.Diagnostics;
using Xunit;

namespace UseNamedArguments.Tests
{
    // TODO: Delegate. class C { void M(System.Action<int, int> f) => f(1, 2);
    // TODO: Indexer. class C { int this[int arg1, int arg2] => this[1, 2]; }
    // TODO: `this` ctor initializer. class C { C(int arg1, int arg2) {} C() : this(1, 2) {} }
    // TODO: `base` ctor initializer. class C { public C(int arg1, int arg2) {} } class D : C { D() : base(1, 2) {} }
    // TODO: ctor. class C { C(int arg1, int arg2) { new C(1, 2); } }
    // TODO: Attribute's parameters and properties?
    public class UseNamedArgumentsAnalyzerTests
    {
        private static class Expect
        {
            /// <summary>
            /// No diagnostics expected to show up for <paramref name="codeSnippet" />
            /// </summary>
            public static void EmptyDiagnosticsFor(string codeSnippet)
            {
                var emptyExpectedDiagnostics = UseNamedArgumentsDiagnosticResult.EmptyExpectedDiagnostics;
                UseNamedArgsCSharpAnalyzerRunner.InvokeAndVerifyDiagnostic(codeSnippet, emptyExpectedDiagnostics);
            }
        }

        [Fact]
        public void Empty_code_does_not_trigger_diagnostic()
            => Expect.EmptyDiagnosticsFor(@"");

        [Fact]
        public void Method_with_zero_args_does_not_trigger_diagnostic()
        {
            const string testCodeSnippet = @"
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

            Expect.EmptyDiagnosticsFor(testCodeSnippet);
        }

        [Fact]
        public void Method_with_one_param_does_not_trigger_diagnostic()
        {
            const string testCodeSnippet = @"
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

            Expect.EmptyDiagnosticsFor(testCodeSnippet);
        }

        [Fact]
        public void Method_with_variable_number_of_params_does_not_trigger_diagnostic()
        {
            const string testCodeSnippet = @"
                namespace Frobnitz`
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

            Expect.EmptyDiagnosticsFor(testCodeSnippet);
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

            Expect.EmptyDiagnosticsFor(testCodeSnippet);
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

            Expect.EmptyDiagnosticsFor(testCodeSnippet);
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

            Expect.EmptyDiagnosticsFor(testCodeSnippet);
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

            Expect.EmptyDiagnosticsFor(testCodeSnippet);
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
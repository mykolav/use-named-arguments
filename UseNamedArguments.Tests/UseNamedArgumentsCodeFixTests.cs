using UseNamedArguments.Tests.Support.CodeFix;
using Xunit;

namespace UseNamedArguments.Tests
{
    public class UseNamedArgumentsCodeFixTests
    {
        [Fact]
        public void Method_with_same_type_params_invocation_with_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
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

            const string fixedCodeSnippet = @"
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

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_with_same_type_params_invocation_with_positional_args_is_fixed_to_named_args_preserving_trivia()
        {
            const string originalCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(string fileName, int line, int column) {}
                        void Bork()
                        {
                            Gork(
                                ""Gizmo.cs"",


                                9000,
                                1);
                        }
                    }
                }
            ";

            const string fixedCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(string fileName, int line, int column) {}
                        void Bork()
                        {
                            Gork(
                                ""Gizmo.cs"",


                                line: 9000,
                                column: 1);
                        }
                    }
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_with_first_two_params_of_same_type_and_third_param_of_another_type_invocation_with_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(int line, int column, string fileName) {}
                        void Bork()
                        {
                            Gork(9000, 1, ""Gizmo.cs"");
                        }
                    }
                }
            ";

            const string fixedCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(int line, int column, string fileName) {}
                        void Bork()
                        {
                            Gork(line: 9000, column: 1, fileName: ""Gizmo.cs"");
                        }
                    }
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_with_first_and_third_params_of_same_type_and_second_param_of_another_type_invocation_with_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(int line, string fileName, int column) {}
                        void Bork()
                        {
                            Gork(9000, ""Gizmo.cs"", 1);
                        }
                    }
                }
            ";

            const string fixedCodeSnippet = @"
                namespace Frobnitz
                {
                    class Wombat
                    {
                        void Gork(int line, string fileName, int column) {}
                        void Bork()
                        {
                            Gork(line: 9000, fileName: ""Gizmo.cs"", column: 1);
                        }
                    }
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }
    }
}

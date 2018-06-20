using UseNamedArguments.Tests.Support.CodeFix;
using Xunit;

namespace UseNamedArguments.Tests
{
    // TODO: Delegate. class C { void M(System.Action<int, int> f) => f(1, 2);
    // TODO: Indexer. class C { int this[int arg1, int arg2] => this[1, 2]; }
    // TODO: `this` ctor initializer. class C { C(int arg1, int arg2) {} C() : this(1, 2) {} }
    // TODO: `base` ctor initializer. class C { public C(int arg1, int arg2) {} } class D : C { D() : base(1, 2) {} }
    // TODO: ctor. class C { C(int arg1, int arg2) { new C(1, 2); } }
    // TODO: Attribute's parameters and properties?
    public class UseNamedArgumentsCodeFixTests
    {
        [Fact]
        public void Method_w_same_type_params_invocation_w_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
                void Gork(string fileName, int line, int column) {}
                void Bork()
                {
                    Gork(""Gizmo.cs"", 9000, 1);
                }
            ";

            const string fixedCodeSnippet = @"
                void Gork(string fileName, int line, int column) {}
                void Bork()
                {
                    Gork(""Gizmo.cs"", line: 9000, column: 1);
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_w_same_type_params_invocation_w_positional_args_is_fixed_to_named_args_preserving_trivia()
        {
            const string originalCodeSnippet = @"
                void Gork(string fileName, int line, int column) {}
                void Bork()
                {
                    Gork(
                        ""Gizmo.cs"",


                        9000,
                        1);
                }
            ";

            const string fixedCodeSnippet = @"
                void Gork(string fileName, int line, int column) {}
                void Bork()
                {
                    Gork(
                        ""Gizmo.cs"",


                        line: 9000,
                        column: 1);
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_w_first_2_params_of_same_type_and_3rd_param_of_another_type_invocation_w_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
                void Gork(int line, int column, string fileName) {}
                void Bork()
                {
                    Gork(9000, 1, ""Gizmo.cs"");
                }
            ";

            const string fixedCodeSnippet = @"
                void Gork(int line, int column, string fileName) {}
                void Bork()
                {
                    Gork(line: 9000, column: 1, fileName: ""Gizmo.cs"");
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_w_1st_and_3rd_params_of_same_type_and_2nd_param_of_another_type_invocation_w_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
                void Gork(int line, string fileName, int column) {}
                void Bork()
                {
                    Gork(9000, ""Gizmo.cs"", 1);
                }
            ";

            const string fixedCodeSnippet = @"
                void Gork(int line, string fileName, int column) {}
                void Bork()
                {
                    Gork(line: 9000, fileName: ""Gizmo.cs"", column: 1);
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_w_3_params_of_same_type_invocation_w_1st_arg_named_and_2_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
                void Gork(string foo, string bar, string baz) {}
                void Bork()
                {
                    Gork(foo: ""pupper"", ""doggo"", ""woofer"");
                }
            ";

            const string fixedCodeSnippet = @"
                void Gork(string foo, string bar, string baz) {}
                void Bork()
                {
                    Gork(foo: ""pupper"", bar: ""doggo"", baz: ""woofer"");
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_w_3_params_of_same_type_invocation_w_2nd_arg_named_and_2_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
                void Gork(string foo, string bar, string baz) {}
                void Bork()
                {
                    Gork(""pupper"", bar: ""doggo"", ""woofer"");
                }
            ";

            const string fixedCodeSnippet = @"
                void Gork(string foo, string bar, string baz) {}
                void Bork()
                {
                    Gork(foo: ""pupper"", bar: ""doggo"", baz: ""woofer"");
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }

        [Fact]
        public void Method_w_3_params_of_same_type_invocation_w_3rd_arg_named_and_2_positional_args_is_fixed_to_named_args()
        {
            const string originalCodeSnippet = @"
                void Gork(string foo, string bar, string baz) {}
                void Bork()
                {
                    Gork(""pupper"", ""doggo"", baz: ""woofer"");
                }
            ";

            const string fixedCodeSnippet = @"
                void Gork(string foo, string bar, string baz) {}
                void Bork()
                {
                    Gork(foo: ""pupper"", bar: ""doggo"", baz: ""woofer"");
                }
            ";

            UseNamedArgsCSharpCodeFixRunner.InvokeAndVerifyResult(originalCodeSnippet, fixedCodeSnippet);
        }
    }
}

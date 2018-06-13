using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UseNamedArguments
{
    internal class ArgumentSyntaxAndParameterSymbol
    {
        public ArgumentSyntaxAndParameterSymbol(ArgumentSyntax argument, IParameterSymbol parameter)
        {
            Argument = argument;
            Parameter = parameter;
        }

        public ArgumentSyntax Argument { get; }
        public IParameterSymbol Parameter { get; }
    }
}

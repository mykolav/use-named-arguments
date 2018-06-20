using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UseNamedArguments
{
    /// <summary>
    /// A container of an <see cref="ArgumentSyntax" /> and
    /// the corresponding <see cref="IParameterSymbol" />.
    /// </summary>
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

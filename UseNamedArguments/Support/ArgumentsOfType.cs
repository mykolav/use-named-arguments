using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UseNamedArguments.Support
{
    /// <summary>
    /// This struct contains <see cref="Arguments" /> which correspond to
    /// parameters of the same <see cref="Type"/>.
    /// </summary>
    internal struct ArgumentsOfType
    {
        public ArgumentsOfType(
            ITypeSymbol type, 
            IReadOnlyList<ArgumentSyntaxAndParameterSymbol> arguments)
        {
            Type = type;
            Arguments = arguments;
        }

        public ITypeSymbol Type { get; }
        public IReadOnlyList<ArgumentSyntaxAndParameterSymbol> Arguments { get; }
    }
}

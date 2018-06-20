using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UseNamedArguments.Support
{
    /// <summary>
    /// Borrowed from https://github.com/DustinCampbell/CSharpEssentials/blob/master/Source/CSharpEssentials/Extensions.cs#L45-L137
    /// Also see https://github.com/dotnet/roslyn/issues/6831
    /// </summary>
    internal static class SemanticModelExtensions
    {
        public static ArgumentInfo GetArgumentInfoOrThrow(
            this SemanticModel semanticModel, 
            ArgumentSyntax argumentSyntax)
        {
            var argumentInfo = semanticModel.GetArgumentInfo(argumentSyntax);
            if (argumentInfo.IsEmpty)
            {
                throw new InvalidOperationException(
                    $"Could not find the corresponding parameter for [{argumentSyntax}]");
            }

            return argumentInfo;
        }

        /// <summary>
        /// To be able to convert positional arguments to named we need to find
        /// corresponding <see cref="IParameterSymbol" /> for each argument.
        /// </summary>
        public static ArgumentInfo GetArgumentInfo(this SemanticModel semanticModel, ArgumentSyntax argument)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            var argumentList = argument.Parent as ArgumentListSyntax;
            var expression = argumentList?.Parent as ExpressionSyntax;
            if (expression == null)
                return default(ArgumentInfo);

            var methodOrProperty = semanticModel.GetSymbolInfo(expression).Symbol;
            if (methodOrProperty == null)
                return default(ArgumentInfo);

            var parameters = methodOrProperty.GetParameters();
            if (parameters.Length == 0)
                return default(ArgumentInfo);

            if (argument.NameColon != null)
            {
                if (argument.NameColon.Name == null)
                    return default(ArgumentInfo);

                // We've got a named argument...
                var nameText = argument.NameColon.Name.Identifier.ValueText;
                if (nameText == null)
                    return default(ArgumentInfo);

                foreach (var parameter in parameters)
                {
                    if (string.Equals(parameter.Name, nameText, StringComparison.Ordinal))
                        return new ArgumentInfo(methodOrProperty, parameter);
                }
            }
            else
            {
                // Positional argument...
                var index = argumentList.Arguments.IndexOf(argument);
                if (index < 0)
                    return default(ArgumentInfo);

                if (index < parameters.Length)
                    return new ArgumentInfo(methodOrProperty, parameters[index]);

                if (index >= parameters.Length &&
                    parameters[parameters.Length - 1].IsParams)
                {
                    return new ArgumentInfo(methodOrProperty, parameters[parameters.Length - 1]);
                }
            }

            return default(ArgumentInfo);
        }

        private static ImmutableArray<IParameterSymbol> GetParameters(this ISymbol symbol)
        {
            switch (symbol?.Kind)
            {
                case SymbolKind.Method:
                    return ((IMethodSymbol)symbol).Parameters;
                case SymbolKind.Property:
                    return ((IPropertySymbol)symbol).Parameters;
                default:
                    return ImmutableArray<IParameterSymbol>.Empty;
            }
        }
    }
}
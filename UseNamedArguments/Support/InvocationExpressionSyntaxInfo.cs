using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UseNamedArguments.Support;

namespace UseNamedArguments
{
    internal class InvocationExpressionSyntaxInfo
    {
        private static readonly IReadOnlyList<(
            ITypeSymbol typeSymbol, 
            List<ArgumentSyntaxAndParameterSymbol> arguments
        )> NoArgumentsShouldBeNamed = new List<(ITypeSymbol, List<ArgumentSyntaxAndParameterSymbol>)>();

        public InvocationExpressionSyntaxInfo(
            IReadOnlyList<(
                ITypeSymbol typeSymbol, 
                List<ArgumentSyntaxAndParameterSymbol> arguments
            )> argumentsWhichShouldBeNamed)
        {
            ArgumentsWhichShouldBeNamed = argumentsWhichShouldBeNamed;
        }

        public IReadOnlyList<(
                    ITypeSymbol typeSymbol, 
                    List<ArgumentSyntaxAndParameterSymbol> arguments
               )> ArgumentsWhichShouldBeNamed { get; }

        public static InvocationExpressionSyntaxInfo From(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var argumentSyntaxes = invocationExpressionSyntax.ArgumentList.Arguments;
            if (argumentSyntaxes.Count == 0)
                return new InvocationExpressionSyntaxInfo(NoArgumentsShouldBeNamed);

            var lastArgumentInfo = GetArgumentInfoOrThrow(semanticModel, argumentSyntaxes.Last());
            if (lastArgumentInfo.Parameter.IsParams)
                return new InvocationExpressionSyntaxInfo(NoArgumentsShouldBeNamed);

            var argumentSyntaxesByTypes = GetArgumentsGroupedByType(semanticModel, argumentSyntaxes);
            var argumentsWhichShouldBeNamed = GetArgumentsWhichShouldBeNamed(argumentSyntaxesByTypes);

            var info = new InvocationExpressionSyntaxInfo(argumentsWhichShouldBeNamed);
            return info;
        }

        private static List<(ITypeSymbol typeSymbol, List<ArgumentSyntaxAndParameterSymbol> arguments)>
            GetArgumentsWhichShouldBeNamed(Dictionary<
                ITypeSymbol, 
                List<ArgumentSyntaxAndParameterSymbol>> argumentSyntaxesByTypes
            )
        {
            var argumentsWhichShouldBeNamedByType = new List<(
                ITypeSymbol typeSymbol,
                List<ArgumentSyntaxAndParameterSymbol> arguments)>();

            foreach (var argumentsOfSameType in argumentSyntaxesByTypes)
            {
                if (argumentsOfSameType.Value.Count(it => it.Argument.NameColon == null) <= 1)
                    continue;

                var argumentNamesSameAsParameterNamesCount = 0;
                foreach (var argument in argumentsOfSameType.Value)
                {
                    if (argument.Argument.Expression is IdentifierNameSyntax identifierNameSyntax &&
                        argument.Parameter.Name == identifierNameSyntax.Identifier.ValueText)
                    {
                        ++argumentNamesSameAsParameterNamesCount;
                    }
                }

                if (argumentNamesSameAsParameterNamesCount == argumentsOfSameType.Value.Count)
                    continue;

                argumentsWhichShouldBeNamedByType.Add((argumentsOfSameType.Key, argumentsOfSameType.Value));
            }

            return argumentsWhichShouldBeNamedByType;
        }

        private static Dictionary<
            ITypeSymbol, 
            List<ArgumentSyntaxAndParameterSymbol>> GetArgumentsGroupedByType(
                SemanticModel semanticModel, 
                SeparatedSyntaxList<ArgumentSyntax> argumentSyntaxes)
        {
            var argumentSyntaxesByTypes = new Dictionary<
                ITypeSymbol,
                List<ArgumentSyntaxAndParameterSymbol>>();

            foreach (var argumentSyntax in argumentSyntaxes)
            {
                ArgumentInfo argumentInfo = GetArgumentInfoOrThrow(semanticModel, argumentSyntax);

                if (!argumentSyntaxesByTypes.TryGetValue(
                    argumentInfo.Parameter.Type,
                    out var argumentSyntaxesOfType))
                {
                    argumentSyntaxesOfType = new List<ArgumentSyntaxAndParameterSymbol>();
                    argumentSyntaxesByTypes.Add(argumentInfo.Parameter.Type, argumentSyntaxesOfType);
                }

                argumentSyntaxesOfType.Add(
                    new ArgumentSyntaxAndParameterSymbol(
                        argumentSyntax,
                        argumentInfo.Parameter)
                );
            }

            return argumentSyntaxesByTypes;
        }

        private static ArgumentInfo GetArgumentInfoOrThrow(
            SemanticModel semanticModel, 
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
    }
}

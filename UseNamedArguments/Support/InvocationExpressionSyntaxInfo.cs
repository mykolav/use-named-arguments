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
            var argumentSyntaxesByType = new Dictionary<ITypeSymbol, List<ArgumentSyntaxAndParameterSymbol>>();
            foreach (var argumentSyntax in invocationExpressionSyntax.ArgumentList.Arguments)
            {
                var argumentInfo = semanticModel.GetArgumentInfo(argumentSyntax);
                if (argumentInfo.IsEmpty)
                    throw new InvalidOperationException($"Could not find the corresponding parameter for [{argumentSyntax}]");

                if (!argumentSyntaxesByType.TryGetValue(argumentInfo.Parameter.Type, out var argumentSyntaxes))
                {
                    argumentSyntaxes = new List<ArgumentSyntaxAndParameterSymbol>();
                    argumentSyntaxesByType.Add(argumentInfo.Parameter.Type, argumentSyntaxes);
                }

                argumentSyntaxes.Add(
                    new ArgumentSyntaxAndParameterSymbol(
                        argumentSyntax, 
                        argumentInfo.Parameter)
                );
            }

            var argumentsWhichShouldBeNamedByType = new List<(
                ITypeSymbol typeSymbol, 
                List<ArgumentSyntaxAndParameterSymbol> arguments)>();

            foreach (var argumentsOfSameType in argumentSyntaxesByType)
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

            var info = new InvocationExpressionSyntaxInfo(argumentsWhichShouldBeNamedByType);
            return info;
        }
    }
}

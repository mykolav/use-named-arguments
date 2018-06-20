using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UseNamedArguments.Support;

namespace UseNamedArguments
{
    /// <summary>
    /// This class contains code to look at an invocation expression and its arguments
    /// and decide whether the arguments should be named.
    /// The rules are:
    ///   - If a method or ctor has a number of parameters of the same type 
    ///     the invocation's corresponding arguments should be named;
    ///   - If named arguments are used for all but one parameter of the same type
    ///     the analyzer doesn't emit the diagnostic;
    ///   - If the last parameter is <see langword="params" />, the analyzer
    ///     doesn't emit the diagnostic, as we cannot use named arguments in this case.
    /// It's used by both 
    ///   - the <see cref="UseNamedArgsForParamsOfSameTypeAnalyzer"/> class and
    ///   - the <see cref="UseNamedArgsForParamsOfSameTypeCodeFixProvider"/> class.
    /// </summary>
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

        /// <summary>
        /// This method analyzes the supplied <paramref name="invocationExpressionSyntax" />
        /// to see if any of the arguments need to be named.
        /// </summary>
        /// <param name="semanticModel">The semantic model is necessary for the analysis</param>
        /// <param name="invocationExpressionSyntax">The invocation to analyze</param>
        /// <returns>
        /// An instance of <see cref="InvocationExpressionSyntaxInfo" /> containing
        /// info <see cref="ArgumentSyntaxAndParameterSymbol" /> about arguments that should be named 
        /// grouped by their types.
        /// </returns>
        public static InvocationExpressionSyntaxInfo From(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var argumentSyntaxes = invocationExpressionSyntax.ArgumentList.Arguments;
            if (argumentSyntaxes.Count == 0)
                return new InvocationExpressionSyntaxInfo(NoArgumentsShouldBeNamed);

            var lastArgumentInfo = semanticModel.GetArgumentInfoOrThrow(argumentSyntaxes.Last());
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
                ArgumentInfo argumentInfo = semanticModel.GetArgumentInfoOrThrow(argumentSyntax);

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
    }
}

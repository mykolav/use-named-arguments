using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UseNamedArguments.Support
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
    internal static class NamedArgumentsExtensions
    {
        private static readonly IReadOnlyList<ArgumentsOfType> NoArgumentsShouldBeNamed = new List<ArgumentsOfType>();

        /// <summary>
        /// This method analyzes the supplied <paramref name="invocationExpressionSyntax" />
        /// to see if any of the arguments need to be named.
        /// </summary>
        /// <param name="semanticModel">The semantic model is necessary for the analysis</param>
        /// <param name="invocationExpressionSyntax">The invocation to analyze</param>
        /// <returns>
        /// An instance of <see cref="NamedArgumentsExtensions" /> containing
        /// info <see cref="ArgumentSyntaxAndParameterSymbol" /> about arguments that should be named 
        /// grouped by their types.
        /// </returns>
        public static IReadOnlyList<ArgumentsOfType> GetArgumentsWhichShouldBeNamed(
            this SemanticModel semanticModel,
            InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var argumentSyntaxes = invocationExpressionSyntax.ArgumentList.Arguments;
            if (argumentSyntaxes.Count == 0)
                return NoArgumentsShouldBeNamed;

            var lastArgumentInfo = semanticModel.GetParameterInfoOrThrow(argumentSyntaxes.Last());
            if (lastArgumentInfo.Parameter.IsParams)
                return NoArgumentsShouldBeNamed;

            var argumentSyntaxesByTypes = argumentSyntaxes
                .Select(syntax => new ArgumentSyntaxAndParameterSymbol(syntax, semanticModel.GetParameterInfoOrThrow(syntax).Parameter))
                .GroupBy(argAndParam => argAndParam.Parameter.Type)
                .Select(group => new ArgumentsOfType(type: group.Key, arguments: group.ToList()));

            var argumentsWhichShouldBeNamed = argumentSyntaxesByTypes
                .Where(it => ShouldArgumentsBeNamed(it.Arguments))
                .ToList();

            return argumentsWhichShouldBeNamed;
        }

        private static bool ShouldArgumentsBeNamed(
            IReadOnlyCollection<ArgumentSyntaxAndParameterSymbol> argumentsOfSameType)
        {
            if (argumentsOfSameType.Count(it => it.Argument.NameColon == null) <= 1)
                return false;

            var argNamesSameAsParamsNamesCount = argumentsOfSameType.Count(
                argAndParam => argAndParam.IsArgumentAndParameterNamesSame());

            return argNamesSameAsParamsNamesCount != argumentsOfSameType.Count;
        }
    }
}

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using UseNamedArguments.Support;

namespace UseNamedArguments
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseNamedArgsForParamsOfSameTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UseNamedArguments";

        // You can change these strings in the Resources.resx file.
        // If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle), 
            Resources.ResourceManager, 
            typeof(Resources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(Resources.AnalyzerMessageFormat), 
            Resources.ResourceManager, 
            typeof(Resources));

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.AnalyzerDescription), 
            Resources.ResourceManager, 
            typeof(Resources));

        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, 
            Title, 
            MessageFormat, 
            Category, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            // context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxNodeAction(
                AnalyzeInvocationOrObjectCreationExpressionNode,
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression);
        }

        public void AnalyzeInvocationOrObjectCreationExpressionNode(SyntaxNodeAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;

            var methodSymbol = semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;
            if (methodSymbol == null)
                return;

            if (!methodSymbol.MethodKind.In(
                    MethodKind.Ordinary, 
                    MethodKind.Constructor, 
                    MethodKind.LocalFunction))
            {
                return;
            }

            var invocationExpressionSyntaxInfo = InvocationExpressionSyntaxInfo.From(
                semanticModel,
                invocationExpressionSyntax);

            if (!invocationExpressionSyntaxInfo.ArgumentsWhichShouldBeNamed.Any())
                return;

            var sbArgumentsOfSameTypeDescriptions = new StringBuilder();
            var argumentsOfSameTypeSeparator = "";
            foreach (var argumentsOfSameType in 
                invocationExpressionSyntaxInfo.ArgumentsWhichShouldBeNamed)
            {
                var argumentsOfSameTypeDescription = string.Join(
                    ", ", 
                    argumentsOfSameType.arguments.Select(it => $"'{it.Parameter.Name}'"));

                sbArgumentsOfSameTypeDescriptions
                    .Append(argumentsOfSameTypeSeparator)
                    .Append(argumentsOfSameTypeDescription);

                argumentsOfSameTypeSeparator = " and ";
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rule, 
                    invocationExpressionSyntax.GetLocation(), 
                    messageArgs: new object[] {
                        methodSymbol.Name, 
                        sbArgumentsOfSameTypeDescriptions.ToString()
                    })
            );
        }
    }
}

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
            // Register ourself to get invoked to analyze 
            //   - invocation expressions; e. g., calling a method. 
            //   - and object creation expressions; e. g., invoking a constructor.
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

            // So far we only support analyzing of the three kinds of methods listed below.
            if (!methodSymbol.MethodKind.In(
                    MethodKind.Ordinary, 
                    MethodKind.Constructor, 
                    MethodKind.LocalFunction))
            {
                return;
            }

            // We got a supported kind of method.
            // Delegate heavy-lifting to the call below.
            var invocationExpressionSyntaxInfo = InvocationExpressionSyntaxInfo.From(
                semanticModel,
                invocationExpressionSyntax);

            // We inspected the arguments of invocation expression.
            // If none of them should be named or, maybe, they already are named,
            // we have nothing more to do. Just return control to the calling code then.
            if (!invocationExpressionSyntaxInfo.ArgumentsWhichShouldBeNamed.Any())
                return;

            // There are arguments that should be named.
            // Prepare the diagnositc's message.
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

            // And finally, emit the diagnostic.
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

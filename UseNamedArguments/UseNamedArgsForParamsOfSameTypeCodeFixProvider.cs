using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using UseNamedArguments.Support;

namespace UseNamedArguments
{
    [ExportCodeFixProvider(
        LanguageNames.CSharp, 
        Name = nameof(UseNamedArgsForParamsOfSameTypeCodeFixProvider))]
    [Shared]
    public class UseNamedArgsForParamsOfSameTypeCodeFixProvider : CodeFixProvider
    {
        private const string title = "Use named args for params of same type";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            UseNamedArgsForParamsOfSameTypeAnalyzer.DiagnosticId
        );

        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md 
        // for more information on Fix All Providers
        public sealed override FixAllProvider GetFixAllProvider() 
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var invocationExpressionSyntax = (InvocationExpressionSyntax)root.FindNode(diagnosticSpan);

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: cancellationToken => 
                        UseNamedArgumentsAsync(
                            context.Document, 
                            root,
                            invocationExpressionSyntax, 
                            cancellationToken), 
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> UseNamedArgumentsAsync(
            Document document,
            SyntaxNode root,
            InvocationExpressionSyntax invocationExpressionSyntax,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync();
            var invocationExpressionSyntaxInfo = InvocationExpressionSyntaxInfo.From(
                semanticModel,
                invocationExpressionSyntax);

            var ordinalOfFirstNamedArgument = invocationExpressionSyntaxInfo
                .ArgumentsWhichShouldBeNamed
                .SelectMany(argumentsByType => argumentsByType.arguments)
                .Min(argAndParam => argAndParam.Parameter.Ordinal);

            var originalArgumentList = invocationExpressionSyntax.ArgumentList;
            var newArgumentSyntaxes = new List<ArgumentSyntax>();
            foreach (var originalArgument in originalArgumentList.Arguments)
            {
                var newArgument = originalArgument;

                var argumentInfo = semanticModel.GetArgumentInfoOrThrow(originalArgument);
                if (argumentInfo.Parameter.Ordinal >= ordinalOfFirstNamedArgument)
                { 
                    newArgument = originalArgument
                        .WithNameColon(
                            SyntaxFactory.NameColon(
                                argumentInfo.Parameter.Name.ToIdentifierName()
                            )
                        )
                        .WithTriviaFrom(originalArgument);
                }

                newArgumentSyntaxes.Add(newArgument);
            }

            var newArguments = SyntaxFactory.SeparatedList(
                newArgumentSyntaxes,
                originalArgumentList.Arguments.GetSeparators());

            var newArgumentList = originalArgumentList.WithArguments(newArguments);
            var newRoot = root.ReplaceNode(originalArgumentList, newArgumentList);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}

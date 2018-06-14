using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace UseNamedArguments.Tests.Support.Analyzer.Diagnostics
{
    internal static class UseNamedArgumentsDiagnosticResult
    {
        public static readonly DiagnosticResult[] EmptyExpectedDiagnostics = {};

        public static DiagnosticResult Create(
            string invokedMethodName, 
            IEnumerable<IEnumerable<string>> parameterNamesByType, 
            params DiagnosticResultLocation[] locations)
        {
            var sbParametersDescriptions = new StringBuilder();
            var parametersDescriptionSeparator = "";
            foreach (var parameterNamesOfSameType in parameterNamesByType)
            {
                sbParametersDescriptions
                    .Append(parametersDescriptionSeparator)
                    .Append(string.Join(", ", parameterNamesOfSameType.Select(it => $"'{it}'")));

                parametersDescriptionSeparator = " and ";
            }

            var parametersDescriptions = sbParametersDescriptions.ToString();

            var message = string.Format(
                UseNamedArguments.Resources.AnalyzerMessageFormat,
                invokedMethodName,
                parametersDescriptions);
            var diagnosticResult = new DiagnosticResult
            {
                Id = UseNamedArgsForParamsOfSameTypeAnalyzer.DiagnosticId,
                Message = message,
                Severity = DiagnosticSeverity.Warning,
                Locations = locations
            };

            return diagnosticResult;
        }
    }
}

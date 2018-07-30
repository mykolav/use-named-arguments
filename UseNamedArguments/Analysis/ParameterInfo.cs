using Microsoft.CodeAnalysis;

namespace UseNamedArguments.Support
{
    /// <summary>
    /// Borrowed from https://github.com/DustinCampbell/CSharpEssentials/blob/master/Source/CSharpEssentials/Extensions.cs#L45-L137
    /// Also see https://github.com/dotnet/roslyn/issues/6831
    /// </summary>
    internal struct ParameterInfo
    {
        public readonly ISymbol MethodOrProperty;
        public readonly IParameterSymbol Parameter;

        public ParameterInfo(ISymbol methodOrProperty, IParameterSymbol parameter)
        {
            MethodOrProperty = methodOrProperty;
            Parameter = parameter;
        }

        public bool IsEmpty => MethodOrProperty == null && Parameter == null;
    }
}

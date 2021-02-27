namespace NativeMock.Analyzer.Shared
{
  using System.Collections.Immutable;
  using Microsoft.CodeAnalysis;

  public static class Rules
  {
    private const string c_designCategory = "Design";

    public static readonly DiagnosticDescriptor NoDeclaringTypeRule = new (
      RuleIds.NoDeclaringTypeRuleId,
      new LocalizableResourceString (nameof(Resources.NoDeclaringTypeTitle), Resources.ResourceManager, typeof(Resources)),
      new LocalizableResourceString (nameof(Resources.NoDeclaringTypeMessageFormat), Resources.ResourceManager, typeof(Resources)),
      c_designCategory,
      DiagnosticSeverity.Error,
      true,
      new LocalizableResourceString (nameof(Resources.NoDeclaringTypeDescription), Resources.ResourceManager, typeof(Resources)));

    public static Diagnostic CreateNoDeclaringTypeDiagnostic (ISymbol method)
    {
      return Diagnostic.Create (NoDeclaringTypeRule, method.Locations[0], method.Name);
    }

    public static readonly DiagnosticDescriptor DeclaringFunctionSignatureMismatchRule = new (
      RuleIds.DeclaringFunctionSignatureMismatchRuleId,
      new LocalizableResourceString (nameof(Resources.DeclaringFunctionSignatureMismatchTitle), Resources.ResourceManager, typeof(Resources)),
      new LocalizableResourceString (nameof(Resources.DeclaringFunctionSignatureMismatchMessageFormat), Resources.ResourceManager, typeof(Resources)),
      c_designCategory,
      DiagnosticSeverity.Error,
      true,
      new LocalizableResourceString (nameof(Resources.DeclaringFunctionSignatureMismatchDescription), Resources.ResourceManager, typeof(Resources)));

    public static Diagnostic CreateDeclaringFunctionSignatureMismatchDiagnostic (ISymbol method)
    {
      return Diagnostic.Create (DeclaringFunctionSignatureMismatchRule, method.Locations[0], method.Name);
    }

    public static ImmutableArray<DiagnosticDescriptor> All { get; } = ImmutableArray.Create (NoDeclaringTypeRule, DeclaringFunctionSignatureMismatchRule);
  }
}

namespace NativeMock.Analyzer
{
  using System.Collections.Immutable;
  using System.Linq;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.Diagnostics;
  using Shared;

  [DiagnosticAnalyzer (LanguageNames.CSharp)]
  public class NativeMockAnalyzer : DiagnosticAnalyzer
  {
    private static readonly NativeMockCallbackProvider s_nativeMockCallbackProvider = new NativeMockCallbackProvider();
    private static readonly PInvokeMemberProvider s_pInvokeMemberProvider = new PInvokeMemberProvider();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules.All;

    public override void Initialize (AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis (GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterSymbolAction (AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol (SymbolAnalysisContext context)
    {
      var namedTypeSymbol = (INamedTypeSymbol) context.Symbol;
      if (namedTypeSymbol.TypeKind != TypeKind.Interface)
        return;

      var nativeMockInterfaceAttribute = AttributeHelper.GetNativeMockInterfaceAttribute (namedTypeSymbol);
      if (nativeMockInterfaceAttribute == null)
        return;

      var moduleName = AttributeHelper.GetModuleNameFromAttribute (nativeMockInterfaceAttribute);
      if (moduleName == null)
        return;

      var defaultDeclaringType = AttributeHelper.GetDeclaringTypeSymbolFromAttribute (nativeMockInterfaceAttribute);
      foreach (var member in namedTypeSymbol.GetMembers())
      {
        if (member.Kind != SymbolKind.Method)
          continue;

        var method = (IMethodSymbol) member;
        var nativeMockCallback = s_nativeMockCallbackProvider.GetNativeMockCallback (method, moduleName, defaultDeclaringType);
        if (nativeMockCallback?.DeclaringType == null)
          continue;

        var members = s_pInvokeMemberProvider.GetPInvokeMembers (nativeMockCallback.DeclaringType);
        var matchingPInvokeMember = members.FirstOrDefault (e => e.Name == nativeMockCallback.Name);

        if (matchingPInvokeMember == null)
        {
          // Cannot find declaring method
          context.ReportDiagnostic (Rules.CreateNoDeclaringTypeDiagnostic (method, nativeMockCallback.Name, nativeMockCallback.DeclaringType));
        }
        else if (!MethodComparer.HaveSameSignature (method, matchingPInvokeMember.Method))
        {
          // Signature mismatch
          context.ReportDiagnostic (Rules.CreateDeclaringFunctionSignatureMismatchDiagnostic (method, matchingPInvokeMember.Method));
        }
      }
    }
  }
}

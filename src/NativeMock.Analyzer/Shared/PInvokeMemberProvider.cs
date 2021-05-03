namespace NativeMock.Analyzer.Shared
{
  using System.Collections.Immutable;
  using Microsoft.CodeAnalysis;

  public class PInvokeMemberProvider
  {
    public ImmutableArray<PInvokeMember> GetPInvokeMembers (ITypeSymbol typeSymbol)
    {
      var builder = ImmutableArray.CreateBuilder<PInvokeMember>();
      foreach (var member in typeSymbol.GetMembers())
      {
        if (member.Kind != SymbolKind.Method || !member.IsExtern)
          continue;

        var method = (IMethodSymbol) member;
        var dllImportData = method.GetDllImportData();
        if (dllImportData?.ModuleName == null)
          continue;

        var identifier = new NativeFunctionIdentifier (dllImportData.ModuleName, dllImportData.EntryPointName ?? method.Name);
        builder.Add (new PInvokeMember (identifier, method));
      }

      return builder.ToImmutable();
    }
  }
}

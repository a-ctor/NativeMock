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

        var identifier = AttributeHelper.GetDllImportIdentifier (member);
        if (identifier == null)
          continue;

        builder.Add (new PInvokeMember (identifier.Value, (IMethodSymbol) member));
      }

      return builder.ToImmutable();
    }
  }
}

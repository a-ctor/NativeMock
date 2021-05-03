namespace NativeMock.Analyzer.Shared
{
  using Microsoft.CodeAnalysis;

  public class PInvokeMember
  {
    public NativeFunctionIdentifier Name { get; }

    public IMethodSymbol Method { get; }

    public PInvokeMember (NativeFunctionIdentifier name, IMethodSymbol method)
    {
      Name = name;
      Method = method;
    }
  }
}

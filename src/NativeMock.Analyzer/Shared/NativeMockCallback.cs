namespace NativeMock.Analyzer.Shared
{
  using Microsoft.CodeAnalysis;

  public class NativeMockCallback
  {
    public NativeFunctionIdentifier Name { get; }

    public ITypeSymbol? DeclaringType { get; }

    public NativeMockCallback (NativeFunctionIdentifier name, ITypeSymbol? declaringType)
    {
      Name = name;
      DeclaringType = declaringType;
    }
  }
}

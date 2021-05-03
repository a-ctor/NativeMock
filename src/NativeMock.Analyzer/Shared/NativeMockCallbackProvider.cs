namespace NativeMock.Analyzer.Shared
{
  using Microsoft.CodeAnalysis;

  public class NativeMockCallbackProvider
  {
    public NativeMockCallback? GetNativeMockCallback (ISymbol symbol, string moduleName, ITypeSymbol? defaultDeclaringType)
    {
      if (symbol.Kind != SymbolKind.Method)
        return null;

      var method = (IMethodSymbol) symbol;
      var nativeMockCallbackAttribute = AttributeHelper.GetNativeMockCallbackAttribute (method);

      var functionName = AttributeHelper.GetNameFromAttribute (nativeMockCallbackAttribute) ?? symbol.Name;
      var declaringType = AttributeHelper.GetDeclaringTypeSymbolFromAttribute (nativeMockCallbackAttribute) ?? defaultDeclaringType;
      return new NativeMockCallback (new NativeFunctionIdentifier (moduleName, functionName), declaringType);
    }
  }
}

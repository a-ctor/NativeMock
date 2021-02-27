namespace NativeMock.Analyzer.Shared
{
  using System.Linq;
  using Microsoft.CodeAnalysis;

  public static class AttributeHelper
  {
    private const string c_nativeMockNamespace = "NativeMock";
    private const string c_nativeMockInterfaceAttributeName = "NativeMockInterfaceAttribute";
    private const string c_nativeMockCallbackAttributeName = "NativeMockCallbackAttribute";

    private const string c_declaringTypePropertyName = "DeclaringType";

    private const string c_dllImportAttributeName = "DllImportAttribute";
    private const string c_dllImportAttributeNamespace = "InteropServices";
    private const string c_dllImportEntryPointPropertyName = "EntryPoint";

    public static NativeFunctionIdentifier? GetDllImportIdentifier (ISymbol? symbol)
    {
      if (symbol == null)
        return null;

      var dllImportAttribute = GetDllImportAttribute (symbol);
      if (dllImportAttribute == null)
        return null;

      var moduleName = GetConstructorArgument (dllImportAttribute, 0) as string;
      var entryPoint = GetNamedArgument (dllImportAttribute, c_dllImportEntryPointPropertyName)?.Value as string;

      return moduleName != null
        ? new NativeFunctionIdentifier (moduleName, entryPoint ?? symbol.Name)
        : null;
    }

    public static string? GetNameFromAttribute (AttributeData? attribute)
    {
      if (attribute == null)
        return null;

      return GetConstructorArgument (attribute, 0) as string;
    }

    public static string? GetModuleNameFromAttribute (AttributeData? attribute)
    {
      if (attribute == null)
        return null;

      return GetConstructorArgument (attribute, 0) as string;
    }

    public static ITypeSymbol? GetDeclaringTypeSymbolFromAttribute (AttributeData? attribute)
    {
      if (attribute == null)
        return null;

      var declaringTypeArgument = GetNamedArgument (attribute, c_declaringTypePropertyName);
      return declaringTypeArgument?.Value as ITypeSymbol;
    }

    private static object? GetConstructorArgument (AttributeData attribute, int position)
    {
      return attribute.ConstructorArguments.Length >= position + 1
        ? attribute.ConstructorArguments[position].Value
        : null;
    }

    private static TypedConstant? GetNamedArgument (AttributeData attribute, string name)
    {
      foreach (var namedArgument in attribute.NamedArguments)
      {
        if (namedArgument.Key != name)
          continue;

        return namedArgument.Value;
      }

      return null;
    }

    public static AttributeData? GetDllImportAttribute (ISymbol symbol) => symbol.GetAttributes().FirstOrDefault (IsDllImportAttribute);

    private static bool IsDllImportAttribute (AttributeData attribute) => attribute.AttributeClass.Name == c_dllImportAttributeName && attribute.AttributeClass.ContainingNamespace.Name == c_dllImportAttributeNamespace;

    public static AttributeData? GetNativeMockInterfaceAttribute (ISymbol symbol) => symbol.GetAttributes().FirstOrDefault (IsNativeMockInterfaceAttribute);

    private static bool IsNativeMockInterfaceAttribute (AttributeData attribute) => attribute.AttributeClass.Name == c_nativeMockInterfaceAttributeName && attribute.AttributeClass.ContainingNamespace.Name == c_nativeMockNamespace;

    public static AttributeData? GetNativeMockCallbackAttribute (ISymbol symbol) => symbol.GetAttributes().FirstOrDefault (IsNativeMockCallbackAttribute);

    private static bool IsNativeMockCallbackAttribute (AttributeData attribute) => attribute.AttributeClass.Name == c_nativeMockCallbackAttributeName && attribute.AttributeClass.ContainingNamespace.Name == c_nativeMockNamespace;
  }
}

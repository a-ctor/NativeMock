namespace NativeMock.SourceGenerator
{
  using System.Linq;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  public class NativeMockInterfaceSyntaxReceiver : ISyntaxReceiver
  {
    private const string c_attributeInferredName = "NativeMockInterface";
    private const string c_attributeExplicitName = c_attributeInferredName + "Attribute";

    public InterfaceDeclarationSyntax InterfaceSyntax { get; private set; }

    /// <inheritdoc />
    public void OnVisitSyntaxNode (SyntaxNode syntaxNode)
    {
      if (!(syntaxNode is InterfaceDeclarationSyntax interfaceSyntax))
        return;

      if (!interfaceSyntax.Modifiers.Any (SyntaxKind.PartialKeyword))
        return;

      var isNativeMockInterface = interfaceSyntax.AttributeLists
        .SelectMany (e => e.Attributes)
        .Any (IsNativeMockInterface);

      // Make sure that the interface is not nested, otherwise there might be problems when generating
      if (!(interfaceSyntax.Parent is NamespaceDeclarationSyntax))
      {
        // todo add a diagnostic for this case
        return;
      }

      if (isNativeMockInterface)
        InterfaceSyntax = interfaceSyntax;
    }

    private bool IsNativeMockInterface (AttributeSyntax attributeSyntax)
    {
      var name = attributeSyntax.Name.ToString();
      return name.EndsWith (c_attributeInferredName) || name.EndsWith (c_attributeExplicitName);
    }
  }
}

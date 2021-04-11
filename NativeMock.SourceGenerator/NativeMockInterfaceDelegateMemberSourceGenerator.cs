namespace NativeMock.SourceGenerator
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  
  [Generator]
  public class NativeMockInterfaceDelegateMemberSourceGenerator : ISourceGenerator
  {
    /// <inheritdoc />
    public void Initialize (GeneratorInitializationContext context)
    {
      context.RegisterForSyntaxNotifications (() => new NativeMockInterfaceSyntaxReceiver());
    }

    /// <inheritdoc />
    public void Execute (GeneratorExecutionContext context)
    {
      var nativeMockInterfaceSyntaxReceiver = (NativeMockInterfaceSyntaxReceiver) context.SyntaxReceiver;
      var interfaceSyntax = nativeMockInterfaceSyntaxReceiver?.InterfaceSyntax;
      if (interfaceSyntax == null)
        return;

      var sb = new StringBuilder();

      var namespaceInformation = GetNamespaceInformation (interfaceSyntax);
      
      sb.AppendLine ($"namespace {namespaceInformation.Namepace}");
      sb.AppendLine ("{");
      foreach (var usingDirectiveSyntax in namespaceInformation.Usings)
      {
        sb.Append ("  ");
        sb.AppendLine (usingDirectiveSyntax.NormalizeWhitespace().ToString());
      }
      if (namespaceInformation.Usings.Count > 0)
        sb.AppendLine();

      sb.AppendLine ($"  partial interface {interfaceSyntax.Identifier.Text}");
      sb.AppendLine ("  {");
      foreach (var method in interfaceSyntax.Members.OfType<MethodDeclarationSyntax>())
      {
        var id = method.Identifier;

        var delegateDeclaration = SyntaxFactory.DelegateDeclaration (
          method.AttributeLists,
          new SyntaxTokenList(),
          SyntaxFactory.Token (SyntaxKind.DelegateKeyword),
          method.ReturnType,
          SyntaxFactory.Identifier (id.Text + "Delegate"),
          method.TypeParameterList,
          method.ParameterList,
          method.ConstraintClauses,
          method.SemicolonToken).NormalizeWhitespace ();

        sb.Append ("    ");
        sb.AppendLine(delegateDeclaration.ToString());
      }
      sb.AppendLine ("  }");
      sb.AppendLine ("}");


      var generatedSource = sb.ToString();
      context.AddSource ($"{namespaceInformation.Namepace}.{interfaceSyntax.Identifier.Text}", generatedSource);
    }

    private static NamepaceInformation GetNamespaceInformation (InterfaceDeclarationSyntax interfaceDeclaration)
    {
      var namespaceDeclarationSyntaxes = new List<NamespaceDeclarationSyntax>();

      var parent = interfaceDeclaration.Parent;
      while (parent != null)
      {
        if (parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
          namespaceDeclarationSyntaxes.Add (namespaceDeclarationSyntax);

        parent = parent.Parent;
      }

      // Reverse the order of the namespaces to match the normal oder Root > SubRoot
      namespaceDeclarationSyntaxes.Reverse();

      var ns = string.Join (".", namespaceDeclarationSyntaxes.Select (e => e.Name.ToString()));
      var usings = SyntaxFactory.List (namespaceDeclarationSyntaxes.SelectMany (e => e.Usings));

      return new NamepaceInformation (ns, usings);
    }
  }
}

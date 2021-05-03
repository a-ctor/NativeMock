namespace NativeMock.SourceGenerator
{
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  public struct NamepaceInformation
  {
    public string Namepace { get; }

    public SyntaxList<UsingDirectiveSyntax> Usings { get; }

    public NamepaceInformation (string namepace, SyntaxList<UsingDirectiveSyntax> usings)
    {
      Namepace = namepace;
      Usings = usings;
    }
  }
}

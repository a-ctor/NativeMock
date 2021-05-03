namespace NativeMock.SourceGenerator.Test.Verifiers
{
  using System.Linq;
  using System.Reflection;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp;
  using NUnit.Framework;

  public static class CSharpCodeGeneratorVerifier<TCodeGenerator>
    where TCodeGenerator : class, ISourceGenerator, new()
  {
    public static void VerifyCodeGenerator (string source, string generated)
    {
      var sourceWithWellKnownTypes = @$"
namespace NativeMock
{{
  public class NativeMockInterfaceAttribute : System.Attribute {{ }}
}}

namespace Test
{{
  {source}
}}";
      var compilation = CreateCompilation (sourceWithWellKnownTypes);

      var sourceGenerator = new TCodeGenerator();
      GeneratorDriver driver = CSharpGeneratorDriver.Create (sourceGenerator);

      driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var outputDiagnostics);

      Assert.That (outputDiagnostics, Is.Empty);
      Assert.That (outputCompilation.GetDiagnostics(), Is.Empty);

      var count = outputCompilation.SyntaxTrees.Count();
      if (generated == null)
      {
        Assert.That (count, Is.EqualTo (1), "The source generator generated an output although no output was expected.");
        return;
      }

      Assert.That (count, Is.EqualTo (2), "The source generator did not generate any outputs.");

      var newSyntaxTree = outputCompilation.SyntaxTrees.Last();
      var newSyntaxTreeCode = newSyntaxTree.ToString();

      Assert.That (generated.TrimStart(), Is.EqualTo (newSyntaxTreeCode));
    }

    private static Compilation CreateCompilation (string source)
    {
      return CSharpCompilation.Create (
        "compilation",
        new[] {CSharpSyntaxTree.ParseText (source)},
        new[] {MetadataReference.CreateFromFile (typeof(Binder).GetTypeInfo().Assembly.Location)},
        new CSharpCompilationOptions (OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
    }
  }
}

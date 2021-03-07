namespace NativeMock.Analyzer.Test
{
  using System.Threading.Tasks;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Shared;
  using VerifyCS = Verifiers.CSharpCodeFixVerifier<NativeMockAnalyzer, NativeMockCallbackSignatureMismatchCodeFixProvider>;

  [TestClass]
  public class NoDeclaringTypeRuleTests
  {
    [TestMethod]
    public async Task NoDiagnosticEmittedWhenNoDeclaringType()
    {
      var test = SourceHelper.Create (
        @"
interface INativeFunctions
{
  void Test();
}");

      await VerifyCS.VerifyAnalyzerAsync (test);
    }

    [TestMethod]
    public async Task DiagnosticEmittedWithDeclaringTypeOnInterface()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
}

[NativeMock.NativeMockInterface (""test.dll"", DeclaringType = typeof(NativeFunctions))]
interface Test
{
  void {|#0:Test|}(); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.NoDeclaringTypeRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "test.dll+Test", "NativeFunctions");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task DiagnosticEmittedWithDeclaringTypeOnMethod()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  void {|#0:Test|}(); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.NoDeclaringTypeRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "test.dll+Test", "NativeFunctions");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task NoDiagnosticEmittedWhenDeclaringMethodExistsWithDeclaringTypeOnInterface()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern void Test();
}

[NativeMock.NativeMockInterface (""test.dll"", DeclaringType = typeof(NativeFunctions))]
interface Test
{
  void Test(); 
}");

      await VerifyCS.VerifyAnalyzerAsync (test);
    }

    [TestMethod]
    public async Task NoDiagnosticEmittedWhenDeclaringMethodExistsWithDeclaringTypeOnMethod()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern void Test();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  void Test(); 
}");

      await VerifyCS.VerifyAnalyzerAsync (test);
    }

    [TestMethod]
    public async Task NoDiagnosticEmittedWithRenamedMethod()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern void Test2();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (""Test2"", DeclaringType = typeof(NativeFunctions))]
  void Test(); 
}");

      await VerifyCS.VerifyAnalyzerAsync (test);
    }

    [TestMethod]
    public async Task DiagnosticEmittedWithRenamedMethodNotFound()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern void Test();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (""Test2"", DeclaringType = typeof(NativeFunctions))]
  void {|#0:Test|}(); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.NoDeclaringTypeRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "test.dll+Test2", "NativeFunctions");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task DiagnosticEmittedWithNonMatchingModuleName()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test2.dll"")]
  public static extern void Test();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  void {|#0:Test|}(); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.NoDeclaringTypeRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "test.dll+Test", "NativeFunctions");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task DiagnosticEmittedWithExternMethodRenamed()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"", EntryPoint = ""Test2"")]
  public static extern void Test3();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  void {|#0:Test|}(); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.NoDeclaringTypeRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "test.dll+Test", "NativeFunctions");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task NoDiagnosticEmittedWithExternMethodRenamed()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"", EntryPoint = ""Test2"")]
  public static extern void Test3();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (""Test2"", DeclaringType = typeof(NativeFunctions))]
  void {|#0:Test|}(); 
}");

      await VerifyCS.VerifyAnalyzerAsync (test);
    }

    [TestMethod]
    public async Task NoDiagnosticEmittedWithExistingCompiledDeclaringType()
    {
      var test = SourceHelper.Create (
        @"
[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(TestAssembly.UnsafeMethods))]
  void Test(); 
}");

      await VerifyCS.VerifyAnalyzerAsync (test);
    }
  }
}

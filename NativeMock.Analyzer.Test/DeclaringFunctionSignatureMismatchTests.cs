namespace NativeMock.Analyzer.Test
{
  using System.Threading.Tasks;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Shared;
  using VerifyCS = Verifiers.CSharpCodeFixVerifier<NativeMockAnalyzer, NativeMockCallbackSignatureMismatchCodeFixProvider>;

  [TestClass]
  public class DeclaringFunctionSignatureMismatchTests
  {
    [TestMethod]
    public async Task ReturnTypeMismatch()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern int Test();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  void {|#0:Test|}(); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task ReturnTypeByRefMismatch()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern ref int Test();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  int {|#0:Test|}(); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task ReturnTypeByRefReadonlyMismatch()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern ref readonly int Test();
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  int {|#0:Test|}(); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task ParameterCountMismatch()
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
  void {|#0:Test|}(int value); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task ParameterTypeMismatch()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern void Test(string value);
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  void {|#0:Test|}(int value); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }

    [TestMethod]
    public async Task ParameterRefMismatch()
    {
      var test = SourceHelper.Create (
        @"
class NativeFunctions
{
  [DllImport(""test.dll"")]
  public static extern void Test(ref int value);
}

[NativeMock.NativeMockInterface (""test.dll"")]
interface Test
{
  [NativeMock.NativeMockCallback (DeclaringType = typeof(NativeFunctions))]
  void {|#0:Test|}(int value); 
}");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test");

      await VerifyCS.VerifyAnalyzerAsync (test, expected);
    }
  }
}

namespace NativeMock.Analyzer.Test
{
  using System.Threading.Tasks;
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using Shared;
  using VerifyCS = Verifiers.CSharpCodeFixVerifier<NativeMockAnalyzer, NativeMockCallbackSignatureMismatchCodeFixProvider>;

  [TestClass]
  public class FixSignatureCodeFixTests
  {
    [TestMethod]
    public async Task FixReturnTypeMismatch()
    {
      var (test, fixedTest) = SourceHelper.CreateForCodeFix (
        "int Test()",
        "void {|#0:Test|}()",
        "int Test()");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "NativeFunctions.Test()");

      await VerifyCS.VerifyCodeFixAsync (test, expected, fixedTest);
    }

    [TestMethod]
    public async Task FixReturnTypeRefMismatch()
    {
      var (test, fixedTest) = SourceHelper.CreateForCodeFix (
        "ref int Test()",
        "int {|#0:Test|}()",
        "ref int Test()");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "NativeFunctions.Test()");

      await VerifyCS.VerifyCodeFixAsync (test, expected, fixedTest);
    }

    [TestMethod]
    public async Task InvalidParameterType()
    {
      var (test, fixedTest) = SourceHelper.CreateForCodeFix (
        "void Test(int test)",
        "void {|#0:Test|}( string  test)",
        "void Test( int  test)");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "NativeFunctions.Test(int)");

      await VerifyCS.VerifyCodeFixAsync (test, expected, fixedTest);
    }

    [TestMethod]
    public async Task InvalidParameterRefType()
    {
      var (test, fixedTest) = SourceHelper.CreateForCodeFix (
        "void Test(ref int test)",
        "void {|#0:Test|}(int test)",
        "void Test(ref int test)");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "NativeFunctions.Test(ref int)");

      await VerifyCS.VerifyCodeFixAsync (test, expected, fixedTest);
    }

    [TestMethod]
    public async Task AddParameterType()
    {
      var (test, fixedTest) = SourceHelper.CreateForCodeFix (
        "void Test(int test)",
        "void {|#0:Test|}()",
        "void Test(int test)");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "NativeFunctions.Test(int)");

      await VerifyCS.VerifyCodeFixAsync (test, expected, fixedTest);
    }

    [TestMethod]
    public async Task AddSecondParameterType()
    {
      var (test, fixedTest) = SourceHelper.CreateForCodeFix (
        "void Test(string value ,  ref     int    test  )",
        "void {|#0:Test|}( string  value)",
        "void Test( string  value, ref int test)");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "NativeFunctions.Test(string, ref int)");

      await VerifyCS.VerifyCodeFixAsync (test, expected, fixedTest);
    }

    [TestMethod]
    public async Task RemoveParameterType()
    {
      var (test, fixedTest) = SourceHelper.CreateForCodeFix (
        "void Test(string value)",
        "void {|#0:Test|}(string value, ref int test)",
        "void Test(string value)");

      var expected = VerifyCS.Diagnostic (RuleIds.DeclaringFunctionSignatureMismatchRuleId)
        .WithLocation (0)
        .WithArguments ("Test", "NativeFunctions.Test(string)");

      await VerifyCS.VerifyCodeFixAsync (test, expected, fixedTest);
    }
  }
}

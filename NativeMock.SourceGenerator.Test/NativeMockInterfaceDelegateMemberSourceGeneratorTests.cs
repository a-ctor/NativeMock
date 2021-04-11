namespace NativeMock.SourceGenerator.Test
{
  using NUnit.Framework;
  using VerifyCS = Verifiers.CSharpCodeGeneratorVerifier<NativeMockInterfaceDelegateMemberSourceGenerator>;

  [TestFixture]
  public class NativeMockInterfaceDelegateMemberSourceGeneratorTests
  {
    [Test]
    public void EmptyString()
    {
      VerifyCS.VerifyCodeGenerator ("", null);
    }

    [Test]
    public void UnannotatedInterface()
    {
      VerifyCS.VerifyCodeGenerator ("interface ITest { }", null);
    }

    [Test]
    public void AnnotatedInterfaceButNotPartial()
    {
      VerifyCS.VerifyCodeGenerator (@"[NativeMock.NativeMockInterface] interface ITest { }", null);
    }

    [Test]
    public void EmptyInterface()
    {
      VerifyCS.VerifyCodeGenerator (
        @"
[NativeMock.NativeMockInterface] 
partial interface ITest 
{
}",
        @"
namespace Test
{
  partial interface ITest
  {
  }
}
");
    }

    [Test]
    public void EmptyInterfaceNestedNamespaces()
    {
      VerifyCS.VerifyCodeGenerator (
        @"
namespace A.B
{
  namespace C
  {
    [NativeMock.NativeMockInterface] 
    partial interface ITest 
    {
    }
  }
}",
        @"
namespace Test.A.B.C
{
  partial interface ITest
  {
  }
}
");
    }

    [Test]
    public void InterfaceWithSingleMember()
    {
      VerifyCS.VerifyCodeGenerator (
        @"
[NativeMock.NativeMockInterface] 
partial interface ITest 
{
  void Test();
}",
        @"
namespace Test
{
  partial interface ITest
  {
    delegate void TestDelegate();
  }
}
");
    }

    [Test]
    public void InterfaceWithMultipleMembers()
    {
      VerifyCS.VerifyCodeGenerator (
        @"
[NativeMock.NativeMockInterface] 
partial interface ITest 
{
  void Test();
  void Test2(int test);
  string Test3(ref int b, out float t);
  System.Text.StringBuilder Test4();
}",
        @"
namespace Test
{
  partial interface ITest
  {
    delegate void TestDelegate();
    delegate void Test2Delegate(int test);
    delegate string Test3Delegate(ref int b, out float t);
    delegate System.Text.StringBuilder Test4Delegate();
  }
}
");
    }

    [Test]
    public void UsingImportsOnSingleLevel()
    {
      VerifyCS.VerifyCodeGenerator (
        @"
using System.Text;

[NativeMock.NativeMockInterface] 
partial interface ITest 
{
  StringBuilder Test();
}",
        @"
namespace Test
{
  using System.Text;

  partial interface ITest
  {
    delegate StringBuilder TestDelegate();
  }
}
");
    }

    [Test]
    public void UsingImportsOnMultipleLevels()
    {
      VerifyCS.VerifyCodeGenerator (
        @"
using System.Text;

namespace A.B
{
  using System.Collections;

  namespace C
  {
    using System.Reflection;

    [NativeMock.NativeMockInterface] 
    partial interface ITest 
    {
      StringBuilder Test();
      IEnumerable Test2();
      FieldAttributes Test3();
    }
  }
}",
        @"
namespace Test.A.B.C
{
  using System.Text;
  using System.Collections;
  using System.Reflection;

  partial interface ITest
  {
    delegate StringBuilder TestDelegate();
    delegate IEnumerable Test2Delegate();
    delegate FieldAttributes Test3Delegate();
  }
}
");
    }
  }
}

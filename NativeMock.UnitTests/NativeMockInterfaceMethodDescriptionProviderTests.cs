namespace NativeMock.UnitTests
{
  using System;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using Infrastructure;
  using NUnit.Framework;

  public class NativeMockInterfaceMethodDescriptionProviderTests
  {
    private interface ITest
    {
      void Empty();

      [NativeMockCallback]
      void EmptyAnnotated();

      [NativeMockCallback ("Test")]
      void Renamed();

      [NativeMockCallback (DeclaringType = typeof(Test))]
      void MissingDeclarationElsewhere();

      [NativeMockCallback (DeclaringType = typeof(Test))]
      void NonExtern();

      [NativeMockCallback (DeclaringType = typeof(Test))]
      void PublicDeclarationElsewhere();

      [NativeMockCallback (DeclaringType = typeof(Test))]
      void PrivateDeclarationElsewhere();

      [NativeMockCallback ("RealRenamedDeclarationElsewhere", DeclaringType = typeof(Test))]
      void RenamedDeclarationElsewhere();

      [NativeMockCallback ("VirtualRenamedDeclarationElsewhere", DeclaringType = typeof(Test))]
      void RealRenamedDeclarationElsewhere2();
    }

    private static class Test
    {
      // ReSharper disable once UnusedMember.Local
      public static void NonExtern()
      {
      }

      [DllImport (FakeDllNames.Dll1)]
      public static extern void PublicDeclarationElsewhere();

      [DllImport (FakeDllNames.Dll1)]
      // ReSharper disable once UnusedMember.Local
      private static extern void PrivateDeclarationElsewhere();

      [DllImport (FakeDllNames.Dll1)]
      public static extern void RealRenamedDeclarationElsewhere();

      [DllImport (FakeDllNames.Dll1, EntryPoint = "VirtualRenamedDeclarationElsewhere")]
      public static extern void VirtualRenamedDeclarationElsewhere_();
    }

    private NativeMockInterfaceMethodDescriptionProvider _nativeMockInterfaceMethodDescriptionProvider;

    [SetUp]
    public void SetUp()
    {
      _nativeMockInterfaceMethodDescriptionProvider = new NativeMockInterfaceMethodDescriptionProvider (new PInvokeMemberProvider());
    }

    [Test]
    public void ThrowsOnNullTest()
    {
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (null!), Throws.ArgumentNullException);
    }

    [Test]
    public void EmptyMethodTest()
    {
      var method = GetInterfaceMethod (e => e.Empty());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo (method.Name));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
    }

    [Test]
    public void EmptyAnnotatedMethodTest()
    {
      var method = GetInterfaceMethod (e => e.EmptyAnnotated());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo (method.Name));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
    }

    [Test]
    public void RenamedTest()
    {
      var method = GetInterfaceMethod (e => e.Renamed());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("Test"));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
    }

    [Test]
    public void MissingDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod (e => e.MissingDeclarationElsewhere());
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod), Throws.InvalidOperationException);
    }

    [Test]
    public void NonExternDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod (e => e.NonExtern());
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod), Throws.InvalidOperationException);
    }

    [Test]
    public void PublicDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod (e => e.PublicDeclarationElsewhere());
      var classMethod = GetClassMethod (Test.PublicDeclarationElsewhere);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("PublicDeclarationElsewhere"));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    [Test]
    public void PrivateDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod (e => e.PrivateDeclarationElsewhere());
      var classMethod = typeof(Test).GetMethod ("PrivateDeclarationElsewhere", BindingFlags.NonPublic | BindingFlags.Static);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("PrivateDeclarationElsewhere"));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    [Test]
    public void RealRenamedDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod (e => e.RenamedDeclarationElsewhere());
      var classMethod = GetClassMethod (Test.RealRenamedDeclarationElsewhere);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("RealRenamedDeclarationElsewhere"));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    [Test]
    public void VirtualRenamedDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod (e => e.RealRenamedDeclarationElsewhere2());
      var classMethod = GetClassMethod (Test.VirtualRenamedDeclarationElsewhere_);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("VirtualRenamedDeclarationElsewhere"));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    private MethodInfo GetInterfaceMethod (Expression<Action<ITest>> lambdaExpression) => ((MethodCallExpression) lambdaExpression.Body).Method;

    private MethodInfo GetClassMethod (Action action) => action.Method;
  }
}

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
      _nativeMockInterfaceMethodDescriptionProvider = new NativeMockInterfaceMethodDescriptionProvider (new PInvokeMemberProvider(), new NativeMockModuleDescriptionProvider());
    }

    [Test]
    public void ThrowsOnNullTest()
    {
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (null!, typeof(int), null), Throws.ArgumentNullException);
    }

    [Test]
    public void EmptyMethodTest()
    {
      var method = GetInterfaceMethod<ITest> (e => e.Empty());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method, null, null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo (method.Name));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
    }

    [Test]
    public void EmptyAnnotatedMethodTest()
    {
      var method = GetInterfaceMethod<ITest> (e => e.EmptyAnnotated());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method, null, null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo (method.Name));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
    }

    [Test]
    public void RenamedTest()
    {
      var method = GetInterfaceMethod<ITest> (e => e.Renamed());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method, null, null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("Test"));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
    }

    [Test]
    public void MissingDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.MissingDeclarationElsewhere());
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod, null, null), Throws.InvalidOperationException);
    }

    [Test]
    public void NonExternDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.NonExtern());
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod, null, null), Throws.InvalidOperationException);
    }

    [Test]
    public void PublicDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.PublicDeclarationElsewhere());
      var classMethod = GetClassMethod (Test.PublicDeclarationElsewhere);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod, null, null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("PublicDeclarationElsewhere"));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    [Test]
    public void PrivateDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.PrivateDeclarationElsewhere());
      var classMethod = typeof(Test).GetMethod ("PrivateDeclarationElsewhere", BindingFlags.NonPublic | BindingFlags.Static);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod, null, null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("PrivateDeclarationElsewhere"));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    [Test]
    public void RealRenamedDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.RenamedDeclarationElsewhere());
      var classMethod = GetClassMethod (Test.RealRenamedDeclarationElsewhere);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod, null, null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("RealRenamedDeclarationElsewhere"));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    [Test]
    public void VirtualRenamedDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.RealRenamedDeclarationElsewhere2());
      var classMethod = GetClassMethod (Test.VirtualRenamedDeclarationElsewhere_);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod, null, null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("VirtualRenamedDeclarationElsewhere"));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    private interface ITest2
    {
      void InheritDeclaringType();

      [NativeMockCallback (DeclaringType = typeof(Test2B))]
      void OverrideDeclaringType();
    }

    private class Test2
    {
      [DllImport (FakeDllNames.Dll1)]
      public static extern void InheritDeclaringType();
    }

    private class Test2B
    {
      [DllImport (FakeDllNames.Dll1)]
      public static extern void OverrideDeclaringType();
    }

    [Test]
    public void InheritDeclaringTypeTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest2> (e => e.InheritDeclaringType());
      var classMethod = GetClassMethod (Test2.InheritDeclaringType);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod, typeof(Test2), null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("InheritDeclaringType"));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    [Test]
    public void OverrideDeclaringTypeTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest2> (e => e.OverrideDeclaringType());
      var classMethod = GetClassMethod (Test2B.OverrideDeclaringType);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (interfaceMethod, typeof(Test2), null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("OverrideDeclaringType"));
      Assert.That (interfaceDescription.Module, Is.Null);
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
    }

    private interface IMismatchInterface
    {
      [NativeMockCallback (nameof(MismatchInterface.ParameterCountMismatch))]
      void ParameterCountMismatch();

      [NativeMockCallback (nameof(MismatchInterface.ReturnTypeMismatch))]
      void ReturnTypeMismatch();

      [NativeMockCallback (nameof(MismatchInterface.ReturnTypeRefMismatch))]
      int ReturnTypeRefMismatch();

      [NativeMockCallback (nameof(MismatchInterface.ParameterMismatch))]
      void ParameterMismatch (string a);

      [NativeMockCallback (nameof(MismatchInterface.ParameterInModifierMismatch))]
      void ParameterInModifierMismatch (in string a);

      [NativeMockCallback (nameof(MismatchInterface.ParameterRefModifierMismatch))]
      void ParameterRefModifierMismatch (ref string a);
    }

    private class MismatchInterface
    {
      [DllImport (FakeDllNames.Dll1)]
      public static extern void ParameterCountMismatch (int a);

      [DllImport (FakeDllNames.Dll1)]
      public static extern int ReturnTypeMismatch();

      [DllImport (FakeDllNames.Dll1)]
      public static extern ref int ReturnTypeRefMismatch();

      [DllImport (FakeDllNames.Dll1)]
      public static extern void ParameterMismatch (int a);

      [DllImport (FakeDllNames.Dll1)]
      public static extern void ParameterInModifierMismatch (ref string a);

      [DllImport (FakeDllNames.Dll1)]
      public static extern void ParameterRefModifierMismatch (in string a);
    }

    [Test]
    public void ParameterCountMismatchTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ParameterCountMismatch());

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          interfaceMethod,
          typeof(MismatchInterface),
          null),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ReturnTypeMismatchTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ReturnTypeMismatch());

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          interfaceMethod,
          typeof(MismatchInterface),
          null),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ReturnTypeRefMismatchTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ReturnTypeRefMismatch());

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          interfaceMethod,
          typeof(MismatchInterface),
          null),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ParameterMismatchTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ParameterMismatch (string.Empty));

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          interfaceMethod,
          typeof(MismatchInterface),
          null),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ParameterInModifierMismatchTest()
    {
      var value = string.Empty;
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ParameterInModifierMismatch (in value));

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          interfaceMethod,
          typeof(MismatchInterface),
          null),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ParameterRefModifierMismatchTest()
    {
      var value = string.Empty;
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ParameterRefModifierMismatch (ref value));

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          interfaceMethod,
          typeof(MismatchInterface),
          null),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    private interface IModuleDefinition
    {
      [NativeMockModule ("asd.dll")]
      [NativeMockCallback]
      void ModuleDefinition();

      [NativeMockModule ("asd.dll")]
      [NativeMockCallback]
      void ModuleDefinitionOverride();
    }

    [Test]
    public void ModuleDefinitionTest()
    {
      var method = GetInterfaceMethod<IModuleDefinition> (e => e.ModuleDefinition());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method, null, null);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("ModuleDefinition"));
      Assert.That (interfaceDescription.Module, Is.EqualTo (new NativeMockModuleDescription ("asd.dll")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
    }

    [Test]
    public void ModuleDefinitionOverrideTest()
    {
      var method = GetInterfaceMethod<IModuleDefinition> (e => e.ModuleDefinitionOverride());
      var defaultModuleDescription = new NativeMockModuleDescription ("other.dll");
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method, null, defaultModuleDescription);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.FunctionName, Is.EqualTo ("ModuleDefinitionOverride"));
      Assert.That (interfaceDescription.Module, Is.EqualTo (new NativeMockModuleDescription ("asd.dll")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
    }

    private MethodInfo GetInterfaceMethod<T> (Expression<Action<T>> lambdaExpression) => ((MethodCallExpression) lambdaExpression.Body).Method;

    private MethodInfo GetClassMethod (Action action) => action.Method;
  }
}

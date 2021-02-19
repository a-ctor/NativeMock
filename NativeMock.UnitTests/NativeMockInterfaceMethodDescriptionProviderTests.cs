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
      var method = GetInterfaceMethod<ITest> (e => e.Empty());
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (null!, method, typeof(int), NativeMockBehavior.Default), Throws.ArgumentNullException);
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription ("abc", null!, typeof(int), NativeMockBehavior.Default), Throws.ArgumentNullException);
    }

    [Test]
    public void ThrowsOnEmptyModuleNameTest()
    {
      var method = GetInterfaceMethod<ITest> (e => e.Empty());
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription ("", method, typeof(int), NativeMockBehavior.Default), Throws.ArgumentException);
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription ("    ", method, typeof(int), NativeMockBehavior.Default), Throws.ArgumentException);
    }

    [Test]
    public void EmptyMethodTest()
    {
      var method = GetInterfaceMethod<ITest> (e => e.Empty());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, method, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, method.Name)));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
    }

    [Test]
    public void EmptyAnnotatedMethodTest()
    {
      var method = GetInterfaceMethod<ITest> (e => e.EmptyAnnotated());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, method, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, method.Name)));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
    }

    [Test]
    public void RenamedTest()
    {
      var method = GetInterfaceMethod<ITest> (e => e.Renamed());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, method, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "Test")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (method));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
    }

    [Test]
    public void MissingDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.MissingDeclarationElsewhere());
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Default), Throws.InvalidOperationException);
    }

    [Test]
    public void NonExternDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.NonExtern());
      Assert.That (() => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Default), Throws.InvalidOperationException);
    }

    [Test]
    public void PublicDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.PublicDeclarationElsewhere());
      var classMethod = GetClassMethod (Test.PublicDeclarationElsewhere);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "PublicDeclarationElsewhere")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
    }

    [Test]
    public void PrivateDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.PrivateDeclarationElsewhere());
      var classMethod = typeof(Test).GetMethod ("PrivateDeclarationElsewhere", BindingFlags.NonPublic | BindingFlags.Static);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "PrivateDeclarationElsewhere")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
    }

    [Test]
    public void RealRenamedDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.RenamedDeclarationElsewhere());
      var classMethod = GetClassMethod (Test.RealRenamedDeclarationElsewhere);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "RealRenamedDeclarationElsewhere")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
    }

    [Test]
    public void VirtualRenamedDeclarationTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest> (e => e.RealRenamedDeclarationElsewhere2());
      var classMethod = GetClassMethod (Test.VirtualRenamedDeclarationElsewhere_);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "VirtualRenamedDeclarationElsewhere")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
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
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, typeof(Test2), NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "InheritDeclaringType")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
    }

    [Test]
    public void OverrideDeclaringTypeTest()
    {
      var interfaceMethod = GetInterfaceMethod<ITest2> (e => e.OverrideDeclaringType());
      var classMethod = GetClassMethod (Test2B.OverrideDeclaringType);
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, typeof(Test2), NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "OverrideDeclaringType")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (classMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
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
          FakeDllNames.Dll1,
          interfaceMethod,
          typeof(MismatchInterface),
          NativeMockBehavior.Default),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ReturnTypeMismatchTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ReturnTypeMismatch());

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          FakeDllNames.Dll1,
          interfaceMethod,
          typeof(MismatchInterface),
          NativeMockBehavior.Default),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ReturnTypeRefMismatchTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ReturnTypeRefMismatch());

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          FakeDllNames.Dll1,
          interfaceMethod,
          typeof(MismatchInterface),
          NativeMockBehavior.Default),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ParameterMismatchTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ParameterMismatch (string.Empty));

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          FakeDllNames.Dll1,
          interfaceMethod,
          typeof(MismatchInterface),
          NativeMockBehavior.Default),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ParameterInModifierMismatchTest()
    {
      var value = string.Empty;
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ParameterInModifierMismatch (in value));

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          FakeDllNames.Dll1,
          interfaceMethod,
          typeof(MismatchInterface),
          NativeMockBehavior.Default),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    [Test]
    public void ParameterRefModifierMismatchTest()
    {
      var value = string.Empty;
      var interfaceMethod = GetInterfaceMethod<IMismatchInterface> (e => e.ParameterRefModifierMismatch (ref value));

      Assert.That (
        () => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
          FakeDllNames.Dll1,
          interfaceMethod,
          typeof(MismatchInterface),
          NativeMockBehavior.Default),
        Throws.TypeOf<NativeMockDeclarationMismatchException>());
    }

    private interface IMockBehavior
    {
      void DefaultBehavior();

      [NativeMockCallback (Behavior = NativeMockBehavior.Loose)]
      void OverrideDefaultBehavior();
    }

    [Test]
    public void DefaultBehaviorTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMockBehavior> (e => e.DefaultBehavior());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "DefaultBehavior")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Default));
    }

    [Test]
    public void OverrideDefaultBehaviorTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMockBehavior> (e => e.OverrideDefaultBehavior());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Default);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "OverrideDefaultBehavior")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Loose));
    }

    [Test]
    public void InheritDefaultBehaviorTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMockBehavior> (e => e.DefaultBehavior());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Strict);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "DefaultBehavior")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Strict));
    }

    [Test]
    public void OverrideInheritedDefaultBehaviorTest()
    {
      var interfaceMethod = GetInterfaceMethod<IMockBehavior> (e => e.OverrideDefaultBehavior());
      var interfaceDescription = _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (FakeDllNames.Dll1, interfaceMethod, null, NativeMockBehavior.Strict);

      Assert.That (interfaceDescription, Is.Not.Null);
      Assert.That (interfaceDescription.Name, Is.EqualTo (new NativeFunctionIdentifier (FakeDllNames.Dll1, "OverrideDefaultBehavior")));
      Assert.That (interfaceDescription.InterfaceMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.StubTargetMethod, Is.EqualTo (interfaceMethod));
      Assert.That (interfaceDescription.Behavior, Is.EqualTo (NativeMockBehavior.Loose));
    }

    private MethodInfo GetInterfaceMethod<T> (Expression<Action<T>> lambdaExpression) => ((MethodCallExpression) lambdaExpression.Body).Method;

    private MethodInfo GetClassMethod (Action action) => action.Method;
  }
}

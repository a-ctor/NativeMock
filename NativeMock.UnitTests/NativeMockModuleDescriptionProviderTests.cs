namespace NativeMock.UnitTests
{
  using System;
  using System.Reflection;
  using System.Text;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockModuleDescriptionProviderTests
  {
    private NativeMockModuleDescriptionProvider _nativeMockInterfaceDescriptionProvider;

    [SetUp]
    public void SetUp()
    {
      _nativeMockInterfaceDescriptionProvider = new NativeMockModuleDescriptionProvider();
    }

    [Test]
    public void GetMockModuleDescriptionForType_ThrowsOnNonInterfaceTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForType (typeof(int)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForType (typeof(ConsoleColor)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForType (typeof(StringBuilder)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForType (typeof(Action)), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void GetMockModuleDescriptionForType_ThrowsOnNullTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForType (null!), Throws.ArgumentNullException);
    }

    private interface IEmpty
    {
    }

    [Test]
    public void GetMockModuleDescriptionForType_ReturnsNullWithoutAnnotationTest()
    {
      var nativeMockModuleDescription = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForType (typeof(IEmpty));

      Assert.That (nativeMockModuleDescription, Is.Null);
    }

    [NativeMockModule ("A")]
    private interface IModuleScoped
    {
    }

    [Test]
    public void GetMockModuleDescriptionForType_ModuleScopedTest()
    {
      var nativeMockModuleDescription = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForType (typeof(IModuleScoped));

      Assert.That (nativeMockModuleDescription, Is.Not.Null);
      Assert.That (nativeMockModuleDescription.Name, Is.EqualTo ("A"));
    }

    [Test]
    public void GetMockModuleDescriptionForMethod_ThrowsOnNullTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForMethod (null!), Throws.ArgumentNullException);
    }

    private static void NonAnnotatedMethod()
    {
    }

    [Test]
    public void GetMockModuleDescriptionForMethod_ReturnsNullWithoutAnnotationTest()
    {
      var nativeMockModuleDescription = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForMethod (GetMethod (nameof(NonAnnotatedMethod)));

      Assert.That (nativeMockModuleDescription, Is.Null);
    }

    [NativeMockModule ("A")]
    private static void AnnotatedMethod()
    {
    }

    [Test]
    public void GetMockModuleDescriptionForMethod_ModuleScopedTest()
    {
      var nativeMockModuleDescription = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescriptionForMethod (GetMethod (nameof(AnnotatedMethod)));

      Assert.That (nativeMockModuleDescription, Is.Not.Null);
      Assert.That (nativeMockModuleDescription.Name, Is.EqualTo ("A"));
    }

    private MethodInfo GetMethod (string name)
    {
      var method = typeof(NativeMockModuleDescriptionProviderTests).GetMethod (name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
      return method ?? throw new InvalidOperationException();
    }
  }
}

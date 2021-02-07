namespace NativeMock.UnitTests
{
  using System;
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
    public void ThrowsOnNonInterfaceTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(int)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(ConsoleColor)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(StringBuilder)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(Action)), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void ThrowsOnNullTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (null!), Throws.ArgumentNullException);
    }

    private interface IEmpty
    {
    }

    [Test]
    public void ReturnsNullWithoutAnnotationTest()
    {
      var nativeMockModuleDescription = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(IEmpty));

      Assert.That (nativeMockModuleDescription, Is.Null);
    }

    [NativeMockModule ("A")]
    private interface IModuleScoped
    {
    }

    [Test]
    public void ModuleScopedTest()
    {
      var nativeMockModuleDescription = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(IModuleScoped));

      Assert.That (nativeMockModuleDescription, Is.Not.Null);
      Assert.That (nativeMockModuleDescription.Name, Is.EqualTo ("A"));
    }
  }
}

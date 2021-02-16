namespace NativeMock.UnitTests
{
  using System;
  using System.Collections.Immutable;
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
      var nativeMockModuleDescriptions = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(IEmpty));

      Assert.That (nativeMockModuleDescriptions, Is.EqualTo (ImmutableArray<NativeMockModuleDescription>.Empty));
    }

    [NativeMockModule ("A")]
    private interface IModuleScoped
    {
    }

    [Test]
    public void ModuleScopedTest()
    {
      var nativeMockModuleDescriptions = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(IModuleScoped));

      Assert.That (nativeMockModuleDescriptions.Length, Is.EqualTo (1));
      Assert.That (nativeMockModuleDescriptions[0].Name, Is.EqualTo ("A"));
    }

    [NativeMockModule ("A")]
    [NativeMockModule ("B")]
    private interface IDoubleModuleScoped
    {
    }

    [Test]
    public void DoubleModuleScopedTest()
    {
      var nativeMockModuleDescriptions = _nativeMockInterfaceDescriptionProvider.GetMockModuleDescription (typeof(IDoubleModuleScoped));

      Assert.That (nativeMockModuleDescriptions.Length, Is.EqualTo (2));
      Assert.That (nativeMockModuleDescriptions[0].Name, Is.EqualTo ("A"));
      Assert.That (nativeMockModuleDescriptions[1].Name, Is.EqualTo ("B"));
    }
  }
}

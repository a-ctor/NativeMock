namespace NativeMock.UnitTests
{
  using System;
  using System.Text;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockInterfaceDescriptionProviderTests
  {
    private interface IEmpty
    {
    }

    private interface IA
    {
      void Test();
    }

    private NativeMockInterfaceDescriptionProvider _nativeMockInterfaceDescriptionProvider;

    [SetUp]
    public void SetUp()
    {
      _nativeMockInterfaceDescriptionProvider = new NativeMockInterfaceDescriptionProvider();
    }

    [Test]
    public void ThrowsOnNonInterface()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(int)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(ConsoleColor)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(StringBuilder)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(Action)), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void ThrowsOnNull()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (null!), Throws.ArgumentNullException);
    }

    [Test]
    public void ThrowsOnEmptyInterface()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(IEmpty)), Throws.InvalidOperationException);
    }

    [Test]
    public void CorrectlyMapsInterfaceMethod()
    {
      var nativeMockInterfaceDescription = _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(IA));

      Assert.That (nativeMockInterfaceDescription, Is.Not.Null);
      Assert.That (nativeMockInterfaceDescription.InterfaceType, Is.SameAs (typeof(IA)));
      Assert.That (nativeMockInterfaceDescription.Methods.Length, Is.EqualTo (1));

      var method = nativeMockInterfaceDescription.Methods[0];
      Assert.That (method.Name, Is.EqualTo (new NativeFunctionIdentifier ("Test")));
      Assert.That (method.MethodInfo, Is.EqualTo (typeof(IA).GetMethod ("Test")));
    }
  }
}

namespace NativeMock.UnitTests
{
  using System;
  using System.Text;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockInterfaceDescriptionProviderTests
  {
    private Mock<INativeMockInterfaceMethodDescriptionProvider> _interfaceMethodDescriptionProviderMock;
    private NativeMockInterfaceDescriptionProvider _nativeMockInterfaceDescriptionProvider;


    [SetUp]
    public void SetUp()
    {
      _interfaceMethodDescriptionProviderMock = new Mock<INativeMockInterfaceMethodDescriptionProvider> (MockBehavior.Strict);
      _nativeMockInterfaceDescriptionProvider = new NativeMockInterfaceDescriptionProvider (
        _interfaceMethodDescriptionProviderMock.Object);
    }

    [Test]
    public void ThrowsOnNonInterfaceTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(int)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(ConsoleColor)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(StringBuilder)), Throws.TypeOf<InvalidOperationException>());
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(Action)), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void ThrowsOnNullTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (null!), Throws.ArgumentNullException);
    }

    private interface IEmpty
    {
    }

    [Test]
    public void ThrowsOnEmptyInterfaceTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(IEmpty)), Throws.InvalidOperationException);
    }

    private interface ISimple
    {
      void Test();
    }

    [Test]
    public void InterfaceWithNoAnnotationThrowsTest()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(ISimple)), Throws.InvalidOperationException);
    }

    [NativeMockInterface ("TestDll", DeclaringType = typeof(int))]
    private interface ISimple2
    {
      void Test();
    }

    [Test]
    public void CorrectlyMapsDeclaringTypeTest()
    {
      var method = typeof(ISimple2).GetMethod ("Test")!;
      var methodDescription = new NativeMockInterfaceMethodDescription (
        new NativeFunctionIdentifier ("TestDll", "Test"),
        method!,
        method!,
        NativeMockBehavior.Default);
      _interfaceMethodDescriptionProviderMock.Setup (m => m.GetMockInterfaceDescription ("TestDll", method, typeof(int), NativeMockBehavior.Default)).Returns (methodDescription);

      var nativeMockInterfaceDescription = _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(ISimple2));

      Assert.That (nativeMockInterfaceDescription, Is.Not.Null);
      Assert.That (nativeMockInterfaceDescription.InterfaceType, Is.SameAs (typeof(ISimple2)));
      Assert.That (nativeMockInterfaceDescription.Methods.Length, Is.EqualTo (1));
      Assert.That (nativeMockInterfaceDescription.Methods[0], Is.EqualTo (methodDescription));

      _interfaceMethodDescriptionProviderMock.VerifyAll();
    }

    [NativeMockInterface ("TestDll", Behavior = NativeMockBehavior.Strict)]
    private interface IMockBehavior
    {
      void Test();
    }

    [Test]
    public void CorrectlyMapsMockBehaviorTest()
    {
      var method = typeof(IMockBehavior).GetMethod ("Test")!;
      var methodDescription = new NativeMockInterfaceMethodDescription (
        new NativeFunctionIdentifier ("TestDll", "Test"),
        method!,
        method!,
        NativeMockBehavior.Strict);
      _interfaceMethodDescriptionProviderMock.Setup (m => m.GetMockInterfaceDescription ("TestDll", method, null, NativeMockBehavior.Strict)).Returns (methodDescription);

      var nativeMockInterfaceDescription = _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(IMockBehavior));

      Assert.That (nativeMockInterfaceDescription, Is.Not.Null);
      Assert.That (nativeMockInterfaceDescription.InterfaceType, Is.SameAs (typeof(IMockBehavior)));
      Assert.That (nativeMockInterfaceDescription.Methods.Length, Is.EqualTo (1));
      Assert.That (nativeMockInterfaceDescription.Methods[0], Is.EqualTo (methodDescription));

      _interfaceMethodDescriptionProviderMock.VerifyAll();
    }
  }
}

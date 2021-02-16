namespace NativeMock.UnitTests
{
  using System;
  using System.Text;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockInterfaceDescriptionProviderTests
  {
    private Mock<INativeMockModuleDescriptionProvider> _moduleDescriptionProviderMock;
    private Mock<INativeMockInterfaceMethodDescriptionProvider> _interfaceMethodDescriptionProviderMock;
    private NativeMockInterfaceDescriptionProvider _nativeMockInterfaceDescriptionProvider;


    [SetUp]
    public void SetUp()
    {
      _moduleDescriptionProviderMock = new Mock<INativeMockModuleDescriptionProvider> (MockBehavior.Strict);
      _interfaceMethodDescriptionProviderMock = new Mock<INativeMockInterfaceMethodDescriptionProvider> (MockBehavior.Strict);
      _nativeMockInterfaceDescriptionProvider = new NativeMockInterfaceDescriptionProvider (
        _moduleDescriptionProviderMock.Object,
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
    public void CorrectlyMapsInterfaceMethodTest()
    {
      var nativeMockModuleDescription = new NativeMockModuleDescription ("Test");
      _moduleDescriptionProviderMock
        .Setup (m => m.GetMockModuleDescriptionForType (typeof(ISimple)))
        .Returns (nativeMockModuleDescription);

      var method = typeof(ISimple).GetMethod ("Test")!;
      var methodDescription = new NativeMockInterfaceMethodDescription (new NativeFunctionIdentifier ("Test", "Test"), method!, method!);
      _interfaceMethodDescriptionProviderMock.Setup (m => m.GetMockInterfaceDescription (method, null, "Test")).Returns (methodDescription);

      var nativeMockInterfaceDescription = _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(ISimple));

      Assert.That (nativeMockInterfaceDescription, Is.Not.Null);
      Assert.That (nativeMockInterfaceDescription.InterfaceType, Is.SameAs (typeof(ISimple)));
      Assert.That (nativeMockInterfaceDescription.Methods.Length, Is.EqualTo (1));
      Assert.That (nativeMockInterfaceDescription.Methods[0], Is.EqualTo (methodDescription));

      _moduleDescriptionProviderMock.VerifyAll();
      _interfaceMethodDescriptionProviderMock.VerifyAll();
    }

    [NativeMockInterface (DeclaringType = typeof(int))]
    private interface ISimple2
    {
      void Test();
    }

    [Test]
    public void CorrectlyMapsInterfaceMethod2Test()
    {
      var method = typeof(ISimple2).GetMethod ("Test")!;
      _moduleDescriptionProviderMock.Setup (e => e.GetMockModuleDescriptionForType (typeof(ISimple2))).Returns ((NativeMockModuleDescription) null);
      var methodDescription = new NativeMockInterfaceMethodDescription (new NativeFunctionIdentifier ("Test"), method!, method!);
      _interfaceMethodDescriptionProviderMock.Setup (m => m.GetMockInterfaceDescription (method, typeof(int), null)).Returns (methodDescription);

      var nativeMockInterfaceDescription = _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(ISimple2));

      Assert.That (nativeMockInterfaceDescription, Is.Not.Null);
      Assert.That (nativeMockInterfaceDescription.InterfaceType, Is.SameAs (typeof(ISimple2)));
      Assert.That (nativeMockInterfaceDescription.Methods.Length, Is.EqualTo (1));
      Assert.That (nativeMockInterfaceDescription.Methods[0], Is.EqualTo (methodDescription));

      _moduleDescriptionProviderMock.VerifyAll();
      _interfaceMethodDescriptionProviderMock.VerifyAll();
    }
  }
}

namespace NativeMock.UnitTests
{
  using System;
  using System.Collections.Immutable;
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
      var nativeMockModuleDescriptions = ImmutableArray<NativeMockModuleDescription>.Empty.Add (new NativeMockModuleDescription ("Test"));
      _moduleDescriptionProviderMock
        .Setup (m => m.GetMockModuleDescription (typeof(ISimple)))
        .Returns (nativeMockModuleDescriptions);

      var method = typeof(ISimple).GetMethod ("Test")!;
      var methodDescription = new NativeMockInterfaceMethodDescription (
        new NativeFunctionIdentifier ("Test", "Test"),
        method!,
        method!,
        NativeMockBehavior.Default);
      _interfaceMethodDescriptionProviderMock.Setup (m => m.GetMockInterfaceDescription (method, null, "Test", NativeMockBehavior.Default)).Returns (methodDescription);

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
      _moduleDescriptionProviderMock.Setup (e => e.GetMockModuleDescription (typeof(ISimple2))).Returns (ImmutableArray<NativeMockModuleDescription>.Empty);
      var methodDescription = new NativeMockInterfaceMethodDescription (
        new NativeFunctionIdentifier ("Test"),
        method!,
        method!,
        NativeMockBehavior.Default);
      _interfaceMethodDescriptionProviderMock.Setup (m => m.GetMockInterfaceDescription (method, typeof(int), null, NativeMockBehavior.Default)).Returns (methodDescription);

      var nativeMockInterfaceDescription = _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(ISimple2));

      Assert.That (nativeMockInterfaceDescription, Is.Not.Null);
      Assert.That (nativeMockInterfaceDescription.InterfaceType, Is.SameAs (typeof(ISimple2)));
      Assert.That (nativeMockInterfaceDescription.Methods.Length, Is.EqualTo (1));
      Assert.That (nativeMockInterfaceDescription.Methods[0], Is.EqualTo (methodDescription));

      _moduleDescriptionProviderMock.VerifyAll();
      _interfaceMethodDescriptionProviderMock.VerifyAll();
    }

    [NativeMockInterface (Behavior = NativeMockBehavior.Strict)]
    private interface IMockBehavior
    {
      void Test();
    }

    [Test]
    public void CorrectlyMapsMockBehaviorTest()
    {
      var method = typeof(IMockBehavior).GetMethod ("Test")!;
      _moduleDescriptionProviderMock.Setup (e => e.GetMockModuleDescription (typeof(IMockBehavior))).Returns (ImmutableArray<NativeMockModuleDescription>.Empty);
      var methodDescription = new NativeMockInterfaceMethodDescription (
        new NativeFunctionIdentifier ("Test"),
        method!,
        method!,
        NativeMockBehavior.Strict);
      _interfaceMethodDescriptionProviderMock.Setup (m => m.GetMockInterfaceDescription (method, null, null, NativeMockBehavior.Strict)).Returns (methodDescription);

      var nativeMockInterfaceDescription = _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(IMockBehavior));

      Assert.That (nativeMockInterfaceDescription, Is.Not.Null);
      Assert.That (nativeMockInterfaceDescription.InterfaceType, Is.SameAs (typeof(IMockBehavior)));
      Assert.That (nativeMockInterfaceDescription.Methods.Length, Is.EqualTo (1));
      Assert.That (nativeMockInterfaceDescription.Methods[0], Is.EqualTo (methodDescription));

      _moduleDescriptionProviderMock.VerifyAll();
      _interfaceMethodDescriptionProviderMock.VerifyAll();
    }
  }
}

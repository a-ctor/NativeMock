namespace NativeMock.UnitTests
{
  using System;
  using System.Text;
  using Moq;
  using NUnit.Framework;
  using Representation;

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
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(int)), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("The specified type must be an interface."));
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(ConsoleColor)), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("The specified type must be an interface."));
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(StringBuilder)), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("The specified type must be an interface."));
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(Action)), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("The specified type must be an interface."));
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

    private interface IParent
    {
    }

    [NativeMockInterface ("TestDll", Behavior = NativeMockBehavior.Strict)]
    private interface IChild : IParent
    {
    }

    [Test]
    public void ThrowsOnInterfaceWithParent()
    {
      Assert.That (() => _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (typeof(IChild)), Throws.ArgumentException.With.Message.StartsWith ("The specified interface type cannot"));
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
        method!,
        method!,
        NativeMockBehavior.Default,
        new NativeFunctionIdentifier ("TestDll", "Test"));
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
        method!,
        method!,
        NativeMockBehavior.Strict,
        new NativeFunctionIdentifier ("TestDll", "Test"));
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

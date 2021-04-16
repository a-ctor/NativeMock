namespace NativeMock.UnitTests
{
  using System;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockTests
  {
    [NativeMockInterface ("sad")]
    public interface ITest
    {
    }

    [Test]
    public void Constructor_ThrowsWhenInterfaceIsNotRegistered()
    {
      var mock = new Mock<ITest>();

      Assert.That (() => new NativeMock<ITest> (mock.Object), Throws.InvalidOperationException.With.Message.StartWith ("The specified"));
    }

    [Test]
    public void Constructor_ThrowsOnNullImplementation()
    {
      Assert.That (() => new NativeMock<ITest> (null!), Throws.ArgumentNullException);
    }

    internal interface IInternalTestApi
    {
      void Write (Span<byte> test);
    }

    public interface ITestApi
    {
      void Write (Span<byte> test);
    }

    [Test]
    public void Constructor_TMustBeAnInterface()
    {
      Assert.That (() => new NativeMock<string>(), Throws.ArgumentException);
      Assert.That (() => new NativeMock<Delegate>(), Throws.ArgumentException);
    }

    [Test]
    public void Constructor_ObjectIsSetAfterConstruction()
    {
      var unsafeMock = new NativeMock<ITestApi>();
      Assert.That (unsafeMock.Object, Is.Not.Null);
    }

    public interface ISimpleTestApi
    {
      void A();

      void B();
    }

    private delegate void TestDelegate();

    [Test]
    public void Verify_All()
    {
      var nativeMock = new NativeMock<ISimpleTestApi>();
      nativeMock.Setup<TestDelegate> (e => e.A, () => { });

      nativeMock.Object.A();
      nativeMock.Verify();
    }

    [Test]
    public void Verify_AllFails()
    {
      var nativeMock = new NativeMock<ISimpleTestApi>();
      nativeMock.Setup<TestDelegate> (e => e.A, () => { });

      Assert.That (() => nativeMock.Verify(), Throws.TypeOf<NativeMockException>());
    }

    [Test]
    public void Verify()
    {
      var nativeMock = new NativeMock<ISimpleTestApi>();
      nativeMock.Setup<TestDelegate> (e => e.A, () => { });

      nativeMock.Object.A();
      nativeMock.Verify<TestDelegate> (e => e.A);
    }

    [Test]
    public void Verify_Fails()
    {
      var nativeMock = new NativeMock<ISimpleTestApi>();
      nativeMock.Setup<TestDelegate> (e => e.A, () => { });

      Assert.That (() => nativeMock.Verify<TestDelegate> (e => e.A), Throws.TypeOf<NativeMockException>());
    }

    [Test]
    public void VerifyAlternate()
    {
      var nativeMock = new NativeMock<ISimpleTestApi>();
      nativeMock.Setup<TestDelegate> (e => e.A, () => { });

      nativeMock.Object.A();
      nativeMock.VerifyAlternate (e => e.A());
    }

    [Test]
    public void VerifyAlternate_Fails()
    {
      var nativeMock = new NativeMock<ISimpleTestApi>();
      nativeMock.Setup<TestDelegate> (e => e.A, () => { });

      Assert.That (() => nativeMock.VerifyAlternate (e => e.A()), Throws.TypeOf<NativeMockException>());
    }
  }
}
